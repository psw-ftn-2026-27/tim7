import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { ClubDto } from '../../api/social-api-types';
import { ClubCard } from '../club-card/club-card';

const PAGE_SIZE = 20;

@Component({
  imports: [ClubCard, RouterLink],
  selector: 'app-club-list',
  styleUrl: './club-list.scss',
  templateUrl: './club-list.html',
})
export class ClubList {
  protected readonly page = signal(1);

  // Reloads from the server whenever the page changes.
  protected readonly clubs = httpResource<PageResult<ClubDto>>(
    () => `/api/social/clubs?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly hasNextPage = computed(() => {
    if (!this.clubs.hasValue()) {
      return false;
    }
    return this.page() * PAGE_SIZE < this.clubs.value().totalCount;
  });

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
