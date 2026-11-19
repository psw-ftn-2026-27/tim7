import { Routes } from '@angular/router';
import { CreateQuiz } from './quiz-management/create-quiz/create-quiz';
import { EditQuiz } from './quiz-management/edit-quiz/edit-quiz';
import { QuizList } from './quiz-management/quiz-list/quiz-list';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: QuizList },
  { path: 'create', component: CreateQuiz },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: ':id/edit', component: EditQuiz },
];
