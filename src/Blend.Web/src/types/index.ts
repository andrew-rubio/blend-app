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

// ── Search & Explore ──────────────────────────────────────────────────────────

/** Indicates where a recipe result originated (EXPL-13). */
export type RecipeDataSource = 'Spoonacular' | 'Community'

/** A single recipe result returned by the unified search endpoint. */
export interface RecipeSearchResult {
  id: string
  title: string
  description?: string
  imageUrl?: string
  readyInMinutes?: number
  servings?: number
  cuisines: string[]
  dishTypes: string[]
  popularity: number
  dataSource: RecipeDataSource
  createdAt?: string
  score: number
}

/** Metadata about a unified search response. */
export interface SearchResponseMetadata {
  totalResults: number
  quotaExhausted: boolean
  nextCursor?: string
}

/** Response envelope from GET /api/v1/search/recipes. */
export interface UnifiedSearchResponse {
  results: RecipeSearchResult[]
  metadata: SearchResponseMetadata
}

/** Active filter state for recipe search (EXPL-11). */
export interface SearchFilters {
  cuisines: string[]
  diets: string[]
  dishTypes: string[]
  maxReadyTime: number | null
}

/** Parameters forwarded to the search API. */
export interface SearchQueryParams {
  q?: string
  cuisines?: string
  diets?: string
  dishTypes?: string
  maxReadyTime?: number
  sort?: string
  cursor?: string
  pageSize?: number
}

// ── Recipe Detail ─────────────────────────────────────────────────────────────

/** A single ingredient in a recipe with amount and unit. */
export interface RecipeIngredient {
  id: string
  name: string
  amount: number
  unit: string
  original?: string
}

/** A single step in a recipe's directions. */
export interface RecipeStep {
  number: number
  step: string
  imageUrl?: string
}

/** Author info for community recipes. */
export interface RecipeAuthor {
  id: string
  name: string
  avatarUrl?: string
}

/** Full recipe detail returned by GET /api/v1/recipes/{id}. */
export interface Recipe {
  id: string
  title: string
  description?: string
  imageUrl?: string
  photos?: string[]
  cuisines: string[]
  dishTypes: string[]
  diets: string[]
  intolerances: string[]
  readyInMinutes?: number
  prepTimeMinutes?: number
  cookTimeMinutes?: number
  servings: number
  difficulty?: 'Easy' | 'Medium' | 'Hard'
  ingredients: RecipeIngredient[]
  steps: RecipeStep[]
  dataSource: RecipeDataSource
  author?: RecipeAuthor
  likeCount: number
  isLiked?: boolean
  viewCount?: number
  createdAt?: string
  updatedAt?: string
}

// ── Cook Mode ─────────────────────────────────────────────────────────────────

export type CookingSessionStatus = 'Active' | 'Paused' | 'Completed'

export interface SessionIngredient {
  ingredientId: string
  name: string
  addedAt: string
  notes?: string
}

export interface CookingSessionDish {
  dishId: string
  name: string
  cuisineType?: string
  ingredients: SessionIngredient[]
  notes?: string
}

export interface CookingSession {
  id: string
  userId: string
  dishes: CookingSessionDish[]
  addedIngredients: SessionIngredient[]
  status: CookingSessionStatus
  createdAt: string
  updatedAt: string
  pausedAt?: string
  ttl?: number
}

export interface SmartSuggestion {
  ingredientId: string
  name: string
  aggregateScore: number
  category?: string
  reason: string
}

export interface SessionSuggestionsResult {
  suggestions: SmartSuggestion[]
  kbUnavailable: boolean
}

export interface IngredientDetailResult {
  ingredientId: string
  name: string
  category?: string
  flavourProfile?: string
  substitutes: string[]
  whyItPairs?: string
  nutritionSummary?: string
}

export interface IngredientSearchResult {
  id: string
  name: string
  category?: string
}

export interface CreateCookSessionRequest {
  recipeId?: string
  initialDishName?: string
}

export interface AddIngredientRequest {
  ingredientId: string
  name: string
  notes?: string
  dishId?: string
}

export interface AddDishRequest {
  name: string
  cuisineType?: string
  notes?: string
}

// ── Wrap-Up Flow ──────────────────────────────────────────────────────────────

export interface PairingFeedbackItem {
  ingredientId1: string
  ingredientId2: string
  /** Star rating from 1 (poor) to 5 (excellent). */
  rating: number
  comment?: string
}

export interface SubmitFeedbackRequest {
  feedback: PairingFeedbackItem[]
}

export interface RecipeDirectionRequest {
  stepNumber: number
  text: string
  mediaUrl?: string
}

export interface PublishSessionRequest {
  title: string
  description?: string
  directions: RecipeDirectionRequest[]
  photos?: string[]
  cuisineType?: string
  tags?: string[]
  servings?: number
  prepTime?: number
  cookTime?: number
}

export interface PublishSessionResult {
  recipeId: string
}

// ── Home Page ─────────────────────────────────────────────────────────────────

export interface HomeSearchSection {
  placeholder: string
}

export interface HomeFeaturedRecipe {
  id: string
  title: string
  imageUrl?: string
  attribution?: string
  shortDescription?: string
}

export interface HomeFeaturedStory {
  id: string
  title: string
  coverImageUrl?: string
  author?: string
  excerpt?: string
  readingTimeMinutes?: number
}

export interface HomeFeaturedVideo {
  id: string
  title: string
  thumbnailUrl?: string
  videoUrl?: string
  creator?: string
}

export interface HomeFeaturedSection {
  recipes: HomeFeaturedRecipe[]
  stories: HomeFeaturedStory[]
  videos: HomeFeaturedVideo[]
}

export interface HomeCommunityRecipe {
  id: string
  title: string
  imageUrl?: string
  authorId?: string
  cuisineType?: string
  likeCount: number
}

export interface HomeCommunitySection {
  recipes: HomeCommunityRecipe[]
}

export interface HomeRecentlyViewedRecipe {
  recipeId: string
  referenceType: string
  viewedAt: string
}

export interface HomeRecentlyViewedSection {
  recipes: HomeRecentlyViewedRecipe[]
}

export interface HomeResponse {
  search: HomeSearchSection
  featured: HomeFeaturedSection
  community: HomeCommunitySection
  recentlyViewed: HomeRecentlyViewedSection
}
