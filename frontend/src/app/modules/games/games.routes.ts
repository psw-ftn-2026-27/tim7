import { Routes } from '@angular/router';
import { CreateQuiz } from './quiz-management/create-quiz/create-quiz';
import { EditQuiz } from './quiz-management/edit-quiz/edit-quiz';
import { QuizList } from './quiz-management/quiz-list/quiz-list';
import { CreateLocationChallenge } from './location-challenge-authoring/create-location-challenge/create-location-challenge';
import { LocationChallengeList } from './location-challenge-authoring/location-challenge-list/location-challenge-list';
import { SolvableQuizList } from './quiz-solving/solvable-quiz-list/solvable-quiz-list';
import { SolveQuiz } from './quiz-solving/solve-quiz/solve-quiz';
import { PlayLocationChallenges } from './location-challenge-playing/play-location-challenges/play-location-challenges';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: QuizList },
  { path: 'create', component: CreateQuiz },
  { path: 'quizzes', component: SolvableQuizList },
  { path: 'quizzes/:id', component: SolveQuiz },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: ':id/edit', component: EditQuiz },
  { path: 'location-challenges', component: LocationChallengeList },
  { path: 'location-challenges/map', component: PlayLocationChallenges },
  { path: 'location-challenges/create', component: CreateLocationChallenge },
];
