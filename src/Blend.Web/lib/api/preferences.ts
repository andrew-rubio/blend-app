import type { UserPreferences, Ingredient } from '@/types/preferences';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? '/api/v1';

export async function getPreferences(): Promise<UserPreferences> {
  const res = await fetch(`${API_URL}/users/me/preferences`);
  if (!res.ok) throw new Error('Failed to fetch preferences');
  return res.json() as Promise<UserPreferences>;
}

export async function savePreferences(prefs: UserPreferences): Promise<void> {
  const res = await fetch(`${API_URL}/users/me/preferences`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(prefs),
  });
  if (!res.ok) throw new Error('Failed to save preferences');
}

export async function searchIngredients(query: string): Promise<Ingredient[]> {
  if (!query.trim()) return [];
  const res = await fetch(`${API_URL}/ingredients/search?query=${encodeURIComponent(query)}`);
  if (!res.ok) throw new Error('Failed to search ingredients');
  return res.json() as Promise<Ingredient[]>;
}
