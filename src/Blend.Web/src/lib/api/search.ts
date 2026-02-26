import { SearchParams, SearchResponse } from '@/types/search';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export async function searchRecipes(params: SearchParams): Promise<SearchResponse> {
  const url = new URL(`${API_URL}/api/v1/search/recipes`);
  url.searchParams.set('q', params.q);
  if (params.filters.cuisines.length > 0) {
    url.searchParams.set('cuisines', params.filters.cuisines.join(','));
  }
  if (params.filters.diets.length > 0) {
    url.searchParams.set('diets', params.filters.diets.join(','));
  }
  if (params.filters.dishTypes.length > 0) {
    url.searchParams.set('dishTypes', params.filters.dishTypes.join(','));
  }
  if (params.filters.maxReadyTime !== undefined) {
    url.searchParams.set('maxReadyTime', String(params.filters.maxReadyTime));
  }
  if (params.cursor) {
    url.searchParams.set('cursor', params.cursor);
  }
  url.searchParams.set('pageSize', String(params.pageSize ?? 20));
  url.searchParams.set('sort', params.sort ?? 'relevance');

  const response = await fetch(url.toString());
  if (!response.ok) {
    throw new Error(`Search failed: ${response.statusText}`);
  }
  return response.json() as Promise<SearchResponse>;
}
