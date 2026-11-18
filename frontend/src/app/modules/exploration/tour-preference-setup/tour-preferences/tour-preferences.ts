import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { MyTourPreferencesDto, SaveTourPreferenceDto } from '../../api/exploration-api-types';
import { TourPreferenceForm } from '../tour-preference-form/tour-preference-form';
import { TourPreferenceSetup } from '../tour-preference-setup';

@Component({
  imports: [TourPreferenceForm],
  selector: 'app-tour-preferences',
  styleUrl: './tour-preferences.scss',
  templateUrl: './tour-preferences.html',
})
export class TourPreferences {
  private readonly tourPreferenceSetup = inject(TourPreferenceSetup);

  protected readonly myPreferences = httpResource<MyTourPreferencesDto>(
    () => '/api/exploration/tour-preferences/mine',
  );

  protected readonly availableTags = httpResource<string[]>(
    () => '/api/exploration/tour-preferences/available-tags',
  );

  protected readonly pending = signal(false);
  protected readonly confirmation = signal<string | null>(null);
  protected readonly message = signal<string | null>(null);
  protected readonly saveError = signal<string | null>(null);

  protected async save(dto: SaveTourPreferenceDto): Promise<void> {
    this.pending.set(true);
    this.confirmation.set(null);
    this.message.set(null);
    this.saveError.set(null);
    try {
      await this.tourPreferenceSetup.save(dto);
      this.confirmation.set('Your preferences are saved.');
      this.myPreferences.reload();
    } catch (failure) {
      this.saveError.set(serverMessage(failure, 'Could not save your preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(): Promise<void> {
    this.pending.set(true);
    this.confirmation.set(null);
    this.message.set(null);
    this.saveError.set(null);
    try {
      await this.tourPreferenceSetup.delete();
      this.myPreferences.reload();
    } catch (failure) {
      if (failure instanceof HttpErrorResponse && failure.status === 404) {
        this.message.set('Your preferences were already deleted.');
        this.myPreferences.reload();
      } else {
        this.message.set(serverMessage(failure, 'Could not delete your preferences.'));
      }
    } finally {
      this.pending.set(false);
    }
  }
}
