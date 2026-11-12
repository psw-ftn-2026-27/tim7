import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import {
  MyTourPreferencesDto,
  SortDirection,
  TourDifficulty,
  TourDto,
  TourSearchDto,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

const PAGE_SIZE = 20;

@Component({
  imports: [FormField, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  private readonly model = signal({
    name: '',
    easy: false,
    moderate: false,
    hard: false,
    tags: '',
    transport: '' as TransportMode | '',
    difficultySort: '' as SortDirection | '',
  });

  protected readonly form = form(this.model);

  protected readonly page = signal(1);

  // The criteria the user entered, in the shape the server expects; null means "not set".
  private readonly search = computed<TourSearchDto>(() => {
    const criteria = this.model();

    const difficulties: TourDifficulty[] = [];
    if (criteria.easy) {
      difficulties.push('Easy');
    }
    if (criteria.moderate) {
      difficulties.push('Moderate');
    }
    if (criteria.hard) {
      difficulties.push('Hard');
    }

    const tags = criteria.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');

    return {
      name: criteria.name === '' ? null : criteria.name,
      difficulties: difficulties.length === 0 ? null : difficulties,
      tags: tags.length === 0 ? null : tags,
      transport: criteria.transport === '' ? null : criteria.transport,
      difficultySort: criteria.difficultySort === '' ? null : criteria.difficultySort,
    };
  });

  // Reloads from the server whenever the criteria or the page change.
  protected readonly tours = httpResource<PageResult<TourDto>>(() => {
    const search = this.search();
    const params = new URLSearchParams();
    params.set('page', String(this.page()));
    params.set('pageSize', String(PAGE_SIZE));
    if (search.name !== null) {
      params.set('name', search.name);
    }
    for (const difficulty of search.difficulties ?? []) {
      params.append('difficulties', difficulty);
    }
    for (const tag of search.tags ?? []) {
      params.append('tags', tag);
    }
    if (search.transport !== null) {
      params.set('transport', search.transport);
    }
    if (search.difficultySort !== null) {
      params.set('difficultySort', search.difficultySort);
    }
    return `/api/exploration/tours/published?${params}`;
  });

  // Only explorers have tour preferences.
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // Read in the background when the page opens; undefined means "do not read".
  protected readonly myPreferences = httpResource<MyTourPreferencesDto>(() =>
    this.isExplorer() ? '/api/exploration/tour-preferences/mine' : undefined,
  );

  protected readonly preferencesNotSetUp = signal(false);

  protected readonly hasNextPage = computed(() => {
    if (!this.tours.hasValue()) {
      return false;
    }
    return this.page() * PAGE_SIZE < this.tours.value().totalCount;
  });

  // Called on every change of a filter or the sort, so the new results start from page 1.
  protected backToFirstPage(): void {
    this.page.set(1);
  }

  // Copies the saved difficulties, transport and tags into the filters; name and sort stay.
  protected applyMyPreferences(): void {
    const preferences = this.myPreferences.value()?.preferences ?? null;
    if (preferences === null) {
      this.preferencesNotSetUp.set(true);
      return;
    }
    const criteria = this.model();
    this.form().reset({
      name: criteria.name,
      easy: preferences.difficulties.includes('Easy'),
      moderate: preferences.difficulties.includes('Moderate'),
      hard: preferences.difficulties.includes('Hard'),
      tags: preferences.tags.join(', '),
      transport: preferences.preferredTransportMode ?? '',
      difficultySort: criteria.difficultySort,
    });
    this.backToFirstPage();
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
