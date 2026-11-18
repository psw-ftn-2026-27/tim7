export type BlogStatus = 'Draft' | 'Published' | 'Archived';

export type BlogVoteType = 'Upvote' | 'Downvote';

export interface CommentDto {
  id: string;
  userId: string;
  text: string;
  createdAt: string;
}

export interface BlogDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  createdAt: string;
  images: string[];
  status: BlogStatus;
  lastModifiedAt: string | null;
  comments: CommentDto[];
  score: number;
  myVote: BlogVoteType | null;
}

export interface CreateBlogDto {
  title: string;
  description: string;
}

export interface EditBlogDto {
  title: string;
  description: string;
  keptImages: string[] | null;
}

export interface CreateCommentDto {
  text: string;
}

export interface UpdateCommentDto {
  text: string;
}

export interface VoteDto {
  type: BlogVoteType;
}

export type ClubStatus = 'Active' | 'Closed';

export interface ClubDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: string[];
  status: ClubStatus;
}

export interface CreateClubDto {
  name: string;
  description: string;
  images: string[] | null;
}

export interface UpdateClubDto {
  name: string;
  description: string;
  images: string[] | null;
}

export type ClubRole = 'Owner' | 'Moderator' | 'Member';

export type MembershipState = 'Member' | 'Pending' | 'None';

export interface ClubMemberDto {
  userId: string;
  role: ClubRole;
}

export interface ClubMembershipDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: string[];
  status: ClubStatus;
  members: ClubMemberDto[];
  myState: MembershipState;
  myRole: ClubRole | null;
  pendingRequests: string[] | null;
}
