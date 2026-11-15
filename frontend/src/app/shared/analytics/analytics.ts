import { Injectable, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { environment } from '../../../environments/environment';

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
// events (RxJS) and touches `document` and `window`. Modules must not copy this.
@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly router = inject(Router);
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

  private sendPageView(): void {
    if (this.decision() !== 'granted' || MEASUREMENT_ID === '' || this.currentPath === null) {
      return;
    }
    gtag('event', 'page_view', {
      page_path: this.currentPath,
      page_location: window.location.origin + this.currentPath,
    });
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
    // Without send_page_view: false Google would record a page view of its own on load.
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
