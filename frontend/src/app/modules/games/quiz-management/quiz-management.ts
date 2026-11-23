import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationDto, QuizDetailsDto, TextDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizManagement {
  private readonly http = inject(HttpClient);

  // Returns the identifier of the new quiz.
  async create(dto: QuizDetailsDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async updateDetails(id: string, dto: QuizDetailsDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async setLocation(id: string, dto: LocationDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/location`, dto));
  }

  async addQuestion(id: string, dto: TextDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/questions`, dto));
  }

  async removeQuestion(id: string, questionId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/questions/${questionId}`));
  }

  async addAnswerOption(id: string, questionId: string, dto: TextDto): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${id}/questions/${questionId}/answer-options`, dto),
    );
  }

  async removeAnswerOption(id: string, questionId: string, answerOptionId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(
        `${BASE_URL}/${id}/questions/${questionId}/answer-options/${answerOptionId}`,
      ),
    );
  }

  async markAnswerOptionCorrect(
    id: string,
    questionId: string,
    answerOptionId: string,
  ): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(
        `${BASE_URL}/${id}/questions/${questionId}/answer-options/${answerOptionId}/mark-correct`,
        {},
      ),
    );
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, {}));
  }

  async restore(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/restore`, {}));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }
}
