export type SocialChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface SocialChallengeDto {
  id: string;
  name: string;
  start: string;
  end: string;
  people: number;
  rangeMeters: number;
  xp: number;
  exitMessage: string;
  status: SocialChallengeStatus;
}

export interface CreateSocialChallengeDto {
  name: string;
  start: string;
  end: string;
  people: number;
  rangeMeters: number;
  xp: number;
  exitMessage: string;
}

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  startLocation: LocationDto;
  xp: number;
  hiddenPointCount: number;
}

export interface HiddenPointDto {
  location: LocationDto | null;
}

export interface CreateLocationChallengeDto {
  name: string;
  xp: number;
  startLocation: LocationDto | null;
  hiddenPoints: HiddenPointDto[] | null;
}

export interface UpdateSocialChallengeDto {
  name: string;
  start: string;
  end: string;
  people: number;
  rangeMeters: number;
  xp: number;
  exitMessage: string;
}
