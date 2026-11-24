import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MapPin } from '../../../../shared/map/map';
import { PositionSimulator } from '../../../../shared/position-simulator/position-simulator';
import { serverMessage } from '../../../../shared/util/server-message';
import { SolvableQuizDto } from '../../api/games-api-types';
import { QuizSolving } from '../quiz-solving';

@Component({
  imports: [PositionSimulator, RouterLink],
  selector: 'app-solvable-quiz-list',
  styleUrl: './solvable-quiz-list.scss',
  templateUrl: './solvable-quiz-list.html',
})
export class SolvableQuizList {
  private readonly quizSolving = inject(QuizSolving);
  private readonly router = inject(Router);

  protected readonly quizzes = httpResource<SolvableQuizDto[]>(() => '/api/games/quizzes/solving');

  // The last position the simulator reported; sent only when the explorer clicks "Reši".
  protected readonly position = signal<MapPin | null>(null);
  protected readonly pending = signal(false);
  // The server message above the list.
  protected readonly error = signal<string | null>(null);

  protected async activate(id: string): Promise<void> {
    const position = this.position();
    if (!position) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizSolving.activate(id, {
        latitude: position.latitude,
        longitude: position.longitude,
      });
      await this.router.navigate(['/games/quizzes', id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije moguće aktivirati.'));
      this.quizzes.reload();
    } finally {
      this.pending.set(false);
    }
  }
}
