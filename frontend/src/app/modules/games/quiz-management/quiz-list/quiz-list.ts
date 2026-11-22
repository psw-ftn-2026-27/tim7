import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ChallengeListItemDto } from '../../api/games-api-types';
import { QuizManagement } from '../quiz-management';

@Component({
  imports: [RouterLink],
  selector: 'app-quiz-list',
  styleUrl: './quiz-list.scss',
  templateUrl: './quiz-list.html',
})
export class QuizList {
  private readonly quizManagement = inject(QuizManagement);
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // Only the first page of the newest 20 challenges; paging controls come when there are more.
  protected readonly challenges = httpResource<PageResult<ChallengeListItemDto>>(
    () => '/api/games/quizzes?page=1&pageSize=20',
  );

  protected readonly pending = signal(false);
  // The server message above the table.
  protected readonly error = signal<string | null>(null);
  // The quiz whose row shows "Confirm delete" and "Cancel" instead of its actions.
  protected readonly confirmingDeleteId = signal<string | null>(null);

  protected async publish(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.publish(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async retire(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.retire(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not retire the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async restore(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.restore(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not restore the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.archive(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not archive the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected askToDelete(id: string): void {
    this.error.set(null);
    this.confirmingDeleteId.set(id);
  }

  protected cancelDelete(): void {
    this.confirmingDeleteId.set(null);
  }

  protected async confirmDelete(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.delete(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the quiz.'));
    } finally {
      // Whether the server deleted it or refused, the row no longer waits for a confirmation.
      this.confirmingDeleteId.set(null);
      this.pending.set(false);
    }
  }
}
