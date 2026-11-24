import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AnswerDto, LocationDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes/solving';

@Injectable({ providedIn: 'root' })
export class QuizSolving {
  private readonly http = inject(HttpClient);

  async activate(id: string, position: LocationDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/activate`, position));
  }

  async answer(id: string, answer: AnswerDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/answers`, answer));
  }
}
