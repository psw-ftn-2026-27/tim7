import { Routes } from '@angular/router';
import { CreateBlog } from './blog-authoring/create-blog/create-blog';
import { MyBlogDetail } from './blog-authoring/my-blog-detail/my-blog-detail';
import { MyBlogs } from './blog-authoring/my-blogs/my-blogs';
import { BlogDetail } from './blog-reading/blog-detail/blog-detail';
import { BlogList } from './blog-reading/blog-list/blog-list';
import { CreateClub } from './club-management/create-club/create-club';
import { MyClubDetail } from './club-management/my-club-detail/my-club-detail';
import { MyClubs } from './club-management/my-clubs/my-clubs';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: 'mine/:id', component: MyBlogDetail },
  { path: 'clubs/mine', component: MyClubs },
  { path: 'clubs/create', component: CreateClub },
  { path: 'clubs/mine/:id', component: MyClubDetail },
  { path: ':id', component: BlogDetail },
];
