import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { CreateLocationChallenge } from './location-challenge-authoring/create-location-challenge/create-location-challenge';
import { LocationChallengeList } from './location-challenge-authoring/location-challenge-list/location-challenge-list';
import { ChallengeList } from './social-challenge-authoring/challenge-list/challenge-list';
import { CreateChallenge } from './social-challenge-authoring/create-challenge/create-challenge';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'challenges/create', component: CreateChallenge },
  { path: 'location-challenges', component: LocationChallengeList },
  { path: 'location-challenges/create', component: CreateLocationChallenge },
];
