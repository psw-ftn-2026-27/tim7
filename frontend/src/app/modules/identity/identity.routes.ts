import { Routes } from '@angular/router';
import { MyFollowing } from './following/my-following/my-following';
import { UserSearch } from './following/user-search/user-search';

export const identityRoutes: Routes = [
  { path: 'users', component: UserSearch },
  { path: 'following', component: MyFollowing },
];
