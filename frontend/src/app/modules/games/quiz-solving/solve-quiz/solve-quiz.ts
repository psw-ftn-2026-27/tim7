import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizSolvingDto } from '../../api/games-api-types';
import { QuizSolving } from '../quiz-solving';

@Component({
  imports: [RouterLink],
  selector: 'app-solve-quiz',
  styleUrl: './solve-quiz.scss',
  templateUrl: './solve-quiz.html',
})
export class SolveQuiz {
  private readonly quizSolving = inject(QuizSolving);

  readonly id = input.required<string>();

  // The first unanswered question while the quiz is unsolved, the result once it is solved.
  protected readonly quiz = httpResource<QuizSolvingDto>(
    () => `/api/games/quizzes/solving/${this.id()}`,
  );

  protected readonly pending = signal(false);
  // The server message above the question.
  protected readonly error = signal<string | null>(null);

  protected async answer(questionId: string, answerOptionId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizSolving.answer(this.id(), { questionId, answerOptionId });
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Odgovor nije sačuvan.'));
    } finally {
      this.pending.set(false);
    }
  }
}
