import { Component, computed, input, output, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import {
  SaveTourPreferenceDto,
  TourDifficulty,
  TourPreferenceDto,
  TransportMode,
} from '../../api/exploration-api-types';

interface TourPreferenceFormModel {
  easy: boolean;
  moderate: boolean;
  hard: boolean;
  transport: TransportMode | '';
  tags: { tag: string; selected: boolean }[];
}

@Component({
  imports: [FormField],
  selector: 'app-tour-preference-form',
  styleUrl: './tour-preference-form.scss',
  templateUrl: './tour-preference-form.html',
})
export class TourPreferenceForm {
  readonly preferences = input.required<TourPreferenceDto | null>();
  readonly availableTags = input.required<string[]>();
  readonly error = input<string | null>(null);
  readonly pending = input(false);

  readonly save = output<SaveTourPreferenceDto>();
  readonly delete = output<void>();

  protected readonly editing = signal(false);

  private readonly model = signal<TourPreferenceFormModel>({
    easy: false,
    moderate: false,
    hard: false,
    transport: '',
    tags: [],
  });

  protected readonly form = form(this.model);

  protected readonly hasCriterion = computed(() => {
    const model = this.model();
    return (
      model.easy ||
      model.moderate ||
      model.hard ||
      model.transport !== '' ||
      model.tags.some((item) => item.selected)
    );
  });

  protected startEdit(): void {
    const preferences = this.preferences();
    const difficulties = preferences?.difficulties ?? [];
    const savedTags = preferences?.tags ?? [];
    const allTags = this.availableTags().concat(
      savedTags.filter((tag) => !this.availableTags().includes(tag)),
    );
    this.form().reset({
      easy: difficulties.includes('Easy'),
      moderate: difficulties.includes('Moderate'),
      hard: difficulties.includes('Hard'),
      transport: preferences?.preferredTransportMode ?? '',
      tags: allTags.map((tag) => ({ tag, selected: savedTags.includes(tag) })),
    });
    this.editing.set(true);
  }

  protected submit(event: Event): void {
    event.preventDefault();
    const model = this.model();
    const difficulties: TourDifficulty[] = [];
    if (model.easy) {
      difficulties.push('Easy');
    }
    if (model.moderate) {
      difficulties.push('Moderate');
    }
    if (model.hard) {
      difficulties.push('Hard');
    }
    this.save.emit({
      difficulties,
      preferredTransportMode: model.transport === '' ? null : model.transport,
      tags: model.tags.filter((item) => item.selected).map((item) => item.tag),
    });
  }
}
