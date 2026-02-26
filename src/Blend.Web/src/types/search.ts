export interface SearchResult {
  id: string;
  title: string;
  image: string;
  cuisines: string[];
  readyInMinutes: number;
  likes: number;
  dataSource: 'spoonacular' | 'community';
}

export interface SearchFilters {
  cuisines: string[];
  diets: string[];
  dishTypes: string[];
  maxReadyTime?: number;
}

export interface SearchResponse {
  results: SearchResult[];
  totalResults: number;
  nextCursor: string | null;
  quotaExhausted: boolean;
}

export interface SearchParams {
  q: string;
  filters: SearchFilters;
  cursor?: string;
  pageSize?: number;
  sort?: string;
}
