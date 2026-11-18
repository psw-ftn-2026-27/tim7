import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTourDetail } from './tour-authoring/my-tour-detail/my-tour-detail';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourDetail } from './tour-browsing/tour-detail/tour-detail';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { TourPreferences } from './tour-preference-setup/tour-preferences/tour-preferences';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id', component: MyTourDetail },
  { path: 'create', component: CreateTour },
  { path: 'preferences', component: TourPreferences },
  { path: ':id', component: TourDetail },
];
