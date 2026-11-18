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

export interface TourLengthDto {
  kilometers: number;
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
  length: TourLengthDto | null;
  transportTimes: TransportTimeDto[];
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: RecommendedGroupSizeDto;
}

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface KeyPointDto {
  id: string;
  order: number;
  name: string;
  description: string;
  image: string;
  location: LocationDto;
  secret: string | null;
  isPublic: boolean;
}

export interface AuthorTourDto {
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
  length: TourLengthDto | null;
  transportTimes: TransportTimeDto[];
  keyPoints: KeyPointDto[];
}

export interface SaveKeyPointDto {
  name: string;
  description: string;
  location: LocationDto;
  secret: string | null;
  isPublic: boolean;
}

export interface PublicKeyPointDto {
  id: string;
  name: string;
  description: string;
  image: string;
  location: LocationDto;
}

export interface PublishedTourDto {
  id: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  length: TourLengthDto | null;
  transportTimes: TransportTimeDto[];
  keyPoints: PublicKeyPointDto[];
}

export interface TourPriceDto {
  isFree: boolean | null;
  amount: number | null;
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
}

export interface TourSearchDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transport: TransportMode | null;
  difficultySort: SortDirection | null;
}
