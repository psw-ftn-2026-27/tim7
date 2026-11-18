import { Routes } from '@angular/router';
import { ChallengeList } from './quiz-management/challenge-list/challenge-list';
import { CreateQuiz } from './quiz-management/create-quiz/create-quiz';
import { EditQuiz } from './quiz-management/edit-quiz/edit-quiz';

export const gamesRoutes: Routes = [
  { path: '', component: ChallengeList },
  { path: 'create', component: CreateQuiz },
  { path: ':id/edit', component: EditQuiz },
];
