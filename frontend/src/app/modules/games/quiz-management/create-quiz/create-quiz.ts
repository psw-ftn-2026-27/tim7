import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizManagement } from '../quiz-management';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-quiz',
  styleUrl: './create-quiz.scss',
  templateUrl: './create-quiz.html',
})
export class CreateQuiz {
  private readonly quizManagement = inject(QuizManagement);
  private readonly router = inject(Router);

  // No client-side validators: the server checks the name and XP and its message is shown.
  protected readonly form = form(
    signal({ name: '', instructions: '', xp: null as number | null }),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const instructions = this.form.instructions().value();
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = await this.quizManagement.create({
        name: this.form.name().value(),
        instructions: instructions === '' ? null : instructions,
        xp: this.form.xp().value(),
      });
      await this.router.navigate(['/games', id, 'edit']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }
}
