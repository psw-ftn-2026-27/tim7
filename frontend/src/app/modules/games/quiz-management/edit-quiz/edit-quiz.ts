import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { AnswerOptionDto, QuestionDto, QuizDto } from '../../api/games-api-types';
import { QuizManagement } from '../quiz-management';
import { QuizQuestion } from '../quiz-question/quiz-question';

@Component({
  imports: [FormField, QuizQuestion, RouterLink],
  selector: 'app-edit-quiz',
  styleUrl: './edit-quiz.scss',
  templateUrl: './edit-quiz.html',
})
export class EditQuiz {
  private readonly quizManagement = inject(QuizManagement);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);

  protected readonly editingDetails = signal(false);
  protected readonly pending = signal(false);
  // The message above the save button of the details form.
  protected readonly detailsError = signal<string | null>(null);
  // The message above the list of questions.
  protected readonly error = signal<string | null>(null);

  // No client-side validators: the server checks the name and XP and its message is shown.
  protected readonly detailsForm = form(
    signal({ name: '', instructions: '', xp: null as number | null }),
  );

  protected readonly questionForm = form(signal({ text: '' }));

  protected startEditDetails(quiz: QuizDto): void {
    this.detailsError.set(null);
    this.detailsForm().reset({
      name: quiz.name,
      instructions: quiz.instructions ?? '',
      xp: quiz.xp,
    });
    this.editingDetails.set(true);
  }

  protected async saveDetails(event: Event): Promise<void> {
    event.preventDefault();
    const instructions = this.detailsForm.instructions().value();
    this.pending.set(true);
    this.detailsError.set(null);
    try {
      await this.quizManagement.updateDetails(this.id(), {
        name: this.detailsForm.name().value(),
        instructions: instructions === '' ? null : instructions,
        xp: this.detailsForm.xp().value(),
      });
      this.editingDetails.set(false);
      this.quiz.reload();
    } catch (failure) {
      this.detailsError.set(serverMessage(failure, 'Could not save the quiz details.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async addQuestion(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.addQuestion(this.id(), { text: this.questionForm.text().value() });
      this.questionForm().reset({ text: '' });
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the question.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async removeQuestion(question: QuestionDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.removeQuestion(this.id(), question.id);
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the question.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async addAnswerOption(question: QuestionDto, text: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.addAnswerOption(this.id(), question.id, { text });
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the answer option.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async removeAnswerOption(
    question: QuestionDto,
    option: AnswerOptionDto,
  ): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.removeAnswerOption(this.id(), question.id, option.id);
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the answer option.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async markCorrect(question: QuestionDto, option: AnswerOptionDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizManagement.markAnswerOptionCorrect(this.id(), question.id, option.id);
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not mark the answer option as correct.'));
    } finally {
      this.pending.set(false);
    }
  }
}
