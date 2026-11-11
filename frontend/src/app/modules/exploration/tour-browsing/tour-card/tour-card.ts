import { Component, computed, input } from '@angular/core';
import { TourDto } from '../../api/exploration-api-types';

const EXCERPT_LENGTH = 150;

@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();

  // The beginning of the description, cut to about 150 characters.
  protected readonly excerpt = computed(() => {
    const description = this.tour().description;
    if (description.length <= EXCERPT_LENGTH) {
      return description;
    }
    return description.slice(0, EXCERPT_LENGTH).trimEnd() + '...';
  });
}
