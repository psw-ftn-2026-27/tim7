import { Component, input, output, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { AnswerOptionDto, QuestionDto } from '../../api/games-api-types';

// Shows one question of a quiz with its answer options and raises every change as an output;
// the edit page runs the command and reloads the quiz.
@Component({
  imports: [FormField],
  selector: 'app-quiz-question',
  styleUrl: './quiz-question.scss',
  templateUrl: './quiz-question.html',
})
export class QuizQuestion {
  readonly question = input.required<QuestionDto>();
  readonly pending = input(false);

  readonly remove = output<void>();
  readonly addAnswerOption = output<string>();
  readonly removeAnswerOption = output<AnswerOptionDto>();
  readonly markCorrect = output<AnswerOptionDto>();

  protected readonly form = form(signal({ text: '' }));

  protected submit(event: Event): void {
    event.preventDefault();
    this.addAnswerOption.emit(this.form.text().value());
    this.form().reset({ text: '' });
  }
}
