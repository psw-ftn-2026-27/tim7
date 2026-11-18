import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { ChallengeListItemDto } from '../../api/games-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  // Only the first page of the newest 20 challenges; paging controls come when there are more.
  protected readonly challenges = httpResource<PageResult<ChallengeListItemDto>>(
    () => '/api/games/quizzes?page=1&pageSize=20',
  );
}
