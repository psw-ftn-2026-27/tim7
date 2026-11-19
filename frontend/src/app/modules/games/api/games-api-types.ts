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

export interface UpdateSocialChallengeDto {
  name: string;
  start: string;
  end: string;
  people: number;
  rangeMeters: number;
  xp: number;
  exitMessage: string;
}
