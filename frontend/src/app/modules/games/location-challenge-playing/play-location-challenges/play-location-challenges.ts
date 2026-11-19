import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Map, MapPin } from '../../../../shared/map/map';
import { PositionSimulator } from '../../../../shared/position-simulator/position-simulator';
import { repeatEvery } from '../../../../shared/util/repeat-every';
import { serverMessage } from '../../../../shared/util/server-message';
import { PlayableLocationChallengeDto, PlayedLocationChallengeDto } from '../../api/games-api-types';
import { LocationChallengePlaying } from '../location-challenge-playing';

const EARTH_RADIUS_METERS = 6_371_000;

// Distance in metres between two points on the Earth (haversine).
function distanceMeters(
  a: { latitude: number; longitude: number },
  b: { latitude: number; longitude: number },
): number {
  const toRadians = (degrees: number) => (degrees * Math.PI) / 180;
  const dLat = toRadians(b.latitude - a.latitude);
  const dLon = toRadians(b.longitude - a.longitude);
  const h =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRadians(a.latitude)) * Math.cos(toRadians(b.latitude)) * Math.sin(dLon / 2) ** 2;
  return 2 * EARTH_RADIUS_METERS * Math.asin(Math.sqrt(h));
}

@Component({
  imports: [Map, PositionSimulator],
  selector: 'app-play-location-challenges',
  styleUrl: './play-location-challenges.scss',
  templateUrl: './play-location-challenges.html',
})
export class PlayLocationChallenges {
  private readonly playing = inject(LocationChallengePlaying);

  protected readonly challenges = httpResource<PlayableLocationChallengeDto[]>(
    () => '/api/games/location-challenges/playing',
  );

  protected readonly panelOpen = signal(false);
  protected readonly selectedId = signal<string | null>(null);
  protected readonly position = signal<MapPin | null>(null);
  protected readonly checkMessage = signal<string | null>(null);
  protected readonly pending = signal(false);

  // Photos and discovered marks of the selected challenge; no request without a selection.
  protected readonly played = httpResource<PlayedLocationChallengeDto>(() => {
    const id = this.selectedId();
    return id ? `/api/games/location-challenges/playing/${id}` : undefined;
  });

  protected readonly startPins = computed<MapPin[]>(() =>
    (this.challenges.value() ?? [])
      .filter((challenge) => challenge.state !== 'Completed')
      .map((challenge) => ({
        latitude: challenge.startLocation.latitude,
        longitude: challenge.startLocation.longitude,
        label: challenge.name,
      })),
  );

  // The selected challenge, unless it is completed: then the panel is back on the list.
  protected readonly selected = computed(() => {
    const challenge = (this.challenges.value() ?? []).find((c) => c.id === this.selectedId());
    return challenge && challenge.state !== 'Completed' ? challenge : null;
  });

  protected readonly circleCenter = computed<MapPin | null>(() => {
    const selected = this.selected();
    return selected ? { ...selected.startLocation } : null;
  });

  protected readonly circleRadiusKm = computed(() => (this.selected()?.radiusMeters ?? 0) / 1000);

  protected readonly selectedPlayable = computed(() => {
    const selected = this.selected();
    return selected !== null && selected.state === 'Active' && this.isInside(selected);
  });

  protected readonly selectedPaused = computed(() => {
    const selected = this.selected();
    return selected !== null && selected.state === 'Active' && !this.isInside(selected);
  });

  // Undiscovered points first; discovered ones sink to the bottom.
  protected readonly hiddenPoints = computed(() => {
    const points = this.played.value()?.hiddenPoints ?? [];
    return [...points].sort((a, b) => Number(a.isDiscovered) - Number(b.isDiscovered));
  });

  constructor() {
    repeatEvery(10_000, () => this.tick());
  }

  protected isInside(challenge: PlayableLocationChallengeDto): boolean {
    const position = this.position();
    return (
      position !== null && distanceMeters(position, challenge.startLocation) <= challenge.radiusMeters
    );
  }

  protected isPaused(challenge: PlayableLocationChallengeDto): boolean {
    return challenge.state === 'Active' && !this.isInside(challenge);
  }

  protected onPositionChanged(position: MapPin | null): void {
    this.position.set(position);
    this.tick();
  }

  protected select(challenge: PlayableLocationChallengeDto): void {
    this.selectedId.set(challenge.id);
    this.checkMessage.set(null);
  }

  protected back(): void {
    this.selectedId.set(null);
    this.checkMessage.set(null);
  }

  protected async check(): Promise<void> {
    const selected = this.selected();
    const position = this.position();
    if (!selected || !position || this.pending()) {
      return;
    }
    this.pending.set(true);
    this.checkMessage.set(null);
    try {
      await this.playing.checkHiddenPoint(selected.id, {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      this.played.reload();
      this.challenges.reload();
    } catch (failure) {
      this.checkMessage.set(serverMessage(failure, 'Nisi na lokaciji skrivene tačke.'));
    } finally {
      this.pending.set(false);
    }
  }

  // Tries to activate every challenge whose circle the user is in, then reads the list again.
  private async tick(): Promise<void> {
    const position = this.position();
    for (const challenge of this.challenges.value() ?? []) {
      if (position && challenge.state === 'NotStarted' && this.isInside(challenge)) {
        try {
          await this.playing.activate(challenge.id, {
            latitude: position.latitude,
            longitude: position.longitude,
          });
        } catch {
          // A refused activation shows no message; the challenge stays inactive.
        }
      }
    }
    this.challenges.reload();
  }
}
