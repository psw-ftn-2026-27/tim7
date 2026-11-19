export type QuizStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface ChallengeListItemDto {
  id: string;
  name: string;
  kind: string;
  status: QuizStatus;
  xp: number | null;
  questionCount: number;
}

export interface QuizDetailsDto {
  name: string;
  instructions: string | null;
  xp: number | null;
}

export interface AnswerOptionDto {
  id: string;
  text: string;
  isCorrect: boolean;
}

export interface QuestionDto {
  id: string;
  text: string;
  answerOptions: AnswerOptionDto[];
}

export interface QuizDto {
  id: string;
  name: string;
  instructions: string | null;
  xp: number | null;
  status: QuizStatus;
  location: LocationDto | null;
  questions: QuestionDto[];
}

export interface TextDto {
  text: string;
}

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

export type LocationChallengeState = 'NotStarted' | 'Active' | 'Completed';

export interface PlayableLocationChallengeDto {
  id: string;
  name: string;
  xp: number;
  startLocation: LocationDto;
  radiusMeters: number;
  state: LocationChallengeState;
}

export interface PlayedHiddenPointDto {
  id: string;
  image: string;
  isDiscovered: boolean;
}

export interface PlayedLocationChallengeDto {
  id: string;
  hiddenPoints: PlayedHiddenPointDto[];
}
