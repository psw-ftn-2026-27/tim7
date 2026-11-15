import { Injectable, computed, inject, isDevMode, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Auth } from '../../core/auth/auth';
import { AnalyticsEvents, AnalyticsUserRole } from './analytics-events';

const CONSENT_KEY = 'explorer.analytics-consent';
const MEASUREMENT_ID = environment.analyticsMeasurementId;
// Google's official opt-out switch: while this property of `window` is true, their script
// sends nothing for this measurement ID.
const OPT_OUT_FLAG = `ga-disable-${MEASUREMENT_ID}` as const;

// 'undecided' means the user has not chosen yet. It is treated as a refusal: nothing is sent.
export type AnalyticsConsent = 'undecided' | 'granted' | 'denied';

// The two properties this service sets on `window` for Google's script.
declare global {
  interface Window {
    dataLayer?: unknown[];
    [optOutFlag: `ga-disable-${string}`]: boolean | undefined;
  }
}

// Google's standard gtag function: it queues a command for their script in window.dataLayer.
// Their script only understands the `arguments` object itself, not an array of parameters.
const gtag: (...parameters: unknown[]) => void = function () {
  window.dataLayer = window.dataLayer ?? [];
  // eslint-disable-next-line prefer-rest-params
  window.dataLayer.push(arguments);
};

// Platform-approved deviation from the Angular subset: this service subscribes to router
// events (RxJS), touches `document` and `window`, and asks `isDevMode()` whether to write to
// the console. Modules must not copy this.
@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  private readonly decision = signal<AnalyticsConsent>(readStoredConsent());

  // The page on screen, set after every completed navigation. Null until the first one.
  private currentPath: string | null = null;
  private scriptAdded = false;

  readonly consent = computed(() => this.decision());

  constructor() {
    if (this.decision() === 'granted') {
      this.startGoogleAnalytics();
    }

    // NavigationEnd also arrives when only a route parameter changes (/social/1 to /social/2).
    // The service lives as long as the application, so it never unsubscribes.
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.currentPath = withoutQueryAndFragment(event.urlAfterRedirects);
        this.sendPageView();
      }
    });
  }

  grant(): void {
    if (this.decision() === 'granted') {
      return;
    }
    this.decide('granted');
    this.startGoogleAnalytics();
    // The page the user is on was opened before they accepted, so its view is recorded now.
    this.sendPageView();
  }

  deny(): void {
    this.decide('denied');
    this.stopGoogleAnalytics();
  }

  private decide(consent: 'granted' | 'denied'): void {
    this.decision.set(consent);
    try {
      localStorage.setItem(CONSENT_KEY, consent);
    } catch {
      // The browser storage is unavailable: the decision holds only while the app is open.
    }
  }

  // Records an event listed in analytics-events.ts. Call it after the action has succeeded,
  // not on click. The compiler rejects a name or a parameter that is not listed. The role and
  // the consent check are added here, and the call never throws: the feature that records an
  // event behaves the same whether recording works or not.
  record<Name extends keyof AnalyticsEvents>(name: Name, parameters: AnalyticsEvents[Name]): void {
    this.send(name, parameters);
  }

  private sendPageView(): void {
    if (this.currentPath === null) {
      return;
    }
    this.send('page_view', {
      page_path: this.currentPath,
      page_location: window.location.origin + this.currentPath,
    });
  }

  // The single path to Google: page views and recorded events both go through here.
  private send(name: string, parameters: Record<string, unknown>): void {
    try {
      if (this.decision() !== 'granted') {
        logInDevelopment(`Analytics event skipped (no consent): ${name}`);
        return;
      }
      if (MEASUREMENT_ID === '') {
        logInDevelopment(`Analytics event skipped (no measurement ID): ${name}`);
        return;
      }

      // The role is written last, so nothing in the parameters can replace it.
      const sent: Record<string, unknown> = { ...parameters, user_role: this.userRole() };
      if (isDevMode()) {
        // Makes the event show up in Google Analytics' DebugView.
        sent['debug_mode'] = true;
      }
      gtag('event', name, sent);
      logInDevelopment(`Analytics event sent: ${name}`, sent);
    } catch (failure) {
      // Recording must never disturb the feature that asked for it.
      logInDevelopment(`Analytics event skipped (recording failed): ${name}`, failure);
    }
  }

  // A user with both roles is recorded with the one that has more permissions.
  private userRole(): AnalyticsUserRole {
    const user = this.auth.user();
    if (user === null) {
      return 'visitor';
    }
    return user.roles.includes('administrator') ? 'administrator' : 'explorer';
  }

  // Loads Google's script the first time consent is given. Later calls only clear the opt-out.
  private startGoogleAnalytics(): void {
    if (MEASUREMENT_ID === '') {
      return;
    }
    window[OPT_OUT_FLAG] = false;
    if (this.scriptAdded) {
      return;
    }
    this.scriptAdded = true;

    gtag('js', new Date());
    // Asks Google not to record a page view of its own on load. Google's tag can ignore this
    // on-page setting, so the same parameter is also set on the tag itself; see the README.
    gtag('config', MEASUREMENT_ID, { send_page_view: false });

    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${MEASUREMENT_ID}`;
    // A blocked or unreachable script must not disturb the user: the page works without it.
    script.onerror = () => console.warn('Google Analytics could not be loaded.');
    document.head.append(script);
  }

  private stopGoogleAnalytics(): void {
    if (MEASUREMENT_ID === '') {
      return;
    }
    window[OPT_OUT_FLAG] = true;
    removeAnalyticsCookies();
  }
}

function readStoredConsent(): AnalyticsConsent {
  try {
    const stored = localStorage.getItem(CONSENT_KEY);
    if (stored === 'granted' || stored === 'denied') {
      return stored;
    }
  } catch {
    // The browser storage is unavailable: there is no decision.
  }
  return 'undecided';
}

// One console line per event, for the team member checking a new event. Development only:
// a production build writes nothing to the console for events.
function logInDevelopment(...line: unknown[]): void {
  if (isDevMode()) {
    console.info(...line);
  }
}

// The part after ? or # can carry personal data (a search term, an email), so it is dropped.
function withoutQueryAndFragment(url: string): string {
  return url.split(/[?#]/)[0];
}

// Google Analytics keeps its cookies under names that start with _ga. A cookie is removed by
// writing it again as expired, with the same path and domain it was set with. Google sets it
// on the widest domain it can, so every parent domain of the host is tried.
function removeAnalyticsCookies(): void {
  const hostParts = window.location.hostname.split('.');
  for (const cookie of document.cookie.split(';')) {
    const name = cookie.split('=')[0].trim();
    if (!name.startsWith('_ga')) {
      continue;
    }
    document.cookie = `${name}=; path=/; max-age=0`;
    for (let start = 0; start < hostParts.length; start++) {
      const domain = hostParts.slice(start).join('.');
      document.cookie = `${name}=; path=/; domain=${domain}; max-age=0`;
    }
  }
}
