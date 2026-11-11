export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export type SortDirection = 'Ascending' | 'Descending';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface RecommendedGroupSizeDto {
  from: number;
  to: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  archivedAt: string | null;
  recommendedGroupSize: RecommendedGroupSizeDto;
  transportTimes: TransportTimeDto[];
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: RecommendedGroupSizeDto;
}

export interface TourPreferenceDto {
  difficulties: TourDifficulty[];
  preferredTransportMode: TransportMode | null;
  tags: string[];
  applyPreferencesAutomatically: boolean;
}

export interface MyTourPreferencesDto {
  preferences: TourPreferenceDto | null;
}

export interface SaveTourPreferenceDto {
  difficulties: TourDifficulty[];
  preferredTransportMode: TransportMode | null;
  tags: string[];
  applyPreferencesAutomatically: boolean;
}
export interface TourSearchDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transport: TransportMode | null;
  difficultySort: SortDirection | null;
}
