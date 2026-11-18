export type RelationshipStatus = 'None' | 'Pending' | 'Following';

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface FoundUserDto {
  id: string;
  email: string;
  status: RelationshipStatus;
}

export interface UserDto {
  id: string;
  email: string;
}

export interface FollowRequestDto {
  userId: string;
  email: string;
  status: FollowRequestStatus;
}

export interface FollowRequestsDto {
  received: FollowRequestDto[];
  sent: FollowRequestDto[];
}
