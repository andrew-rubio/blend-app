export interface User {
  id: string
  email: string
  name: string
  role: 'user' | 'admin'
  createdAt: string
}

export interface ApiError {
  message: string
  status: number
  code?: string
}

export interface PaginatedResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export type ThemeMode = 'light' | 'dark' | 'system'

// ── User Preferences ──────────────────────────────────────────────────────────

export interface UserPreferences {
  favoriteCuisines: string[]
  favoriteDishTypes: string[]
  diets: string[]
  intolerances: string[]
  dislikedIngredientIds: string[]
}

export interface UpdatePreferencesRequest {
  favoriteCuisines: string[]
  favoriteDishTypes: string[]
  diets: string[]
  intolerances: string[]
  dislikedIngredientIds: string[]
}

export interface PreferenceLists {
  cuisines: string[]
  dishTypes: string[]
  diets: string[]
  intolerances: string[]
}

/** A selectable ingredient item used in the disliked ingredients typeahead. */
export interface IngredientItem {
  id: string
  name: string
}
