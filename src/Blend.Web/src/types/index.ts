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
export interface RecipeNutritionInfo {
  calories?: number
  protein?: number
  carbs?: number
  fat?: number
  fiber?: number
  sugar?: number
}

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
  nutritionInfo?: RecipeNutritionInfo
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

// ── Profile ───────────────────────────────────────────────────────────────────

export interface MyProfile {
  id: string
  displayName: string
  email: string
  avatarUrl?: string
  bio?: string
  joinDate: string
  recipeCount: number
  likeCount: number
  followerCount: number
  followingCount: number
}

export interface PublicProfile {
  id: string
  displayName: string
  avatarUrl?: string
  bio?: string
  joinDate: string
  recipeCount: number
}

export interface UpdateProfileRequest {
  displayName: string
  bio?: string
  avatarUrl?: string
}

export interface ProfileRecipe {
  id: string
  title: string
  imageUrl?: string
  cuisines: string[]
  likeCount: number
  isPublic: boolean
  createdAt: string
}

export interface ProfileRecipesResponse {
  recipes: ProfileRecipe[]
  nextCursor?: string
  hasMore: boolean
}

// ── Friends ───────────────────────────────────────────────────────────────────

export interface FriendItem {
  userId: string
  displayName: string
  avatarUrl?: string
  recipeCount: number
  connectedAt: string
}

export interface FriendRequestItem {
  requestId: string
  userId: string
  displayName: string
  avatarUrl?: string
  sentAt: string
}

export type ConnectionStatus = 'none' | 'friends' | 'pending_sent' | 'pending_received'

export interface UserSearchResultItem {
  userId: string
  displayName: string
  avatarUrl?: string
  recipeCount: number
  connectionStatus: ConnectionStatus
}

export interface FriendsPageResponse {
  items: FriendItem[]
  nextCursor?: string
  hasNextPage: boolean
}

export interface FriendRequestsPageResponse {
  items: FriendRequestItem[]
  nextCursor?: string
  hasNextPage: boolean
}

export interface UserSearchPageResponse {
  items: UserSearchResultItem[]
  nextCursor?: string
  hasNextPage: boolean
}

// ── Notifications ──────────────────────────────────────────────────────────────

export type NotificationType =
  | 'friendRequestReceived'
  | 'friendRequestAccepted'
  | 'recipeLiked'
  | 'recipePublished'

export interface ApiNotificationItem {
  id: string
  type: NotificationType
  actorDisplayName?: string
  actorAvatarUrl?: string
  recipeTitle?: string
  recipeId?: string
  targetUserId?: string
  isRead: boolean
  createdAt: string
}

export interface NotificationsPageResponse {
  items: ApiNotificationItem[]
  nextCursor?: string
  hasNextPage: boolean
}

export interface UnreadCountResponse {
  count: number
}

// ── Settings ──────────────────────────────────────────────────────────────────

export type UnitSystem = 'Metric' | 'Imperial'

export interface AppSettings {
  unitSystem: UnitSystem
  theme: ThemeMode
}

export interface UpdateSettingsRequest {
  unitSystem?: UnitSystem
  theme?: ThemeMode
}

// ── Ingredient Submissions ────────────────────────────────────────────────────

export type IngredientSubmissionStatus = 'Pending' | 'Approved' | 'Rejected'

export type IngredientCategory =
  | 'Produce'
  | 'Meat'
  | 'Seafood'
  | 'Dairy'
  | 'Grains'
  | 'Spices'
  | 'Condiments'
  | 'Beverages'
  | 'Other'

export interface IngredientSubmission {
  id: string
  name: string
  category: IngredientCategory
  description?: string
  status: IngredientSubmissionStatus
  submittedAt: string
  reviewedAt?: string
  reviewNotes?: string
}

export interface CreateIngredientSubmissionRequest {
  name: string
  category: IngredientCategory
  description?: string
}

export interface IngredientSubmissionsResponse {
  submissions: IngredientSubmission[]
}

// ── Ingredient Catalogue ──────────────────────────────────────────────────────

export interface CatalogueIngredient {
  id: string
  name: string
  category?: string
  flavourProfile?: string
}

export interface IngredientCatalogueResponse {
  ingredients: CatalogueIngredient[]
  nextCursor?: string
  hasMore: boolean
}

// ── Account Deletion ──────────────────────────────────────────────────────────

export interface DeleteAccountRequest {
  password?: string
}

export interface DeleteAccountResponse {
  scheduledDeletionDate: string
}

export interface CancelDeletionResponse {
  message: string
}

// ── Admin Content Management ──────────────────────────────────────────────────

export interface AdminFeaturedRecipe {
  id: string
  recipeId: string
  title: string
  description?: string
  coverImageUrl?: string
  displayOrder: number
  source: 'Spoonacular' | 'Community'
  createdAt: string
}

export interface CreateFeaturedRecipeRequest {
  recipeId: string
  title: string
  description?: string
  coverImageUrl?: string
  displayOrder: number
}

export interface UpdateFeaturedRecipeRequest {
  title?: string
  description?: string
  coverImageUrl?: string
  displayOrder?: number
}

export interface AdminStory {
  id: string
  title: string
  author: string
  content: string
  coverImageUrl?: string
  readingTimeMinutes: number
  relatedRecipeIds: string[]
  publishedAt?: string
  createdAt: string
}

export interface CreateStoryRequest {
  title: string
  author: string
  content: string
  coverImageUrl?: string
  readingTimeMinutes: number
  relatedRecipeIds: string[]
}

export interface UpdateStoryRequest {
  title?: string
  author?: string
  content?: string
  coverImageUrl?: string
  readingTimeMinutes?: number
  relatedRecipeIds?: string[]
}

export interface AdminVideo {
  id: string
  title: string
  creator: string
  embedUrl: string
  thumbnailUrl?: string
  durationSeconds?: number
  createdAt: string
}

export interface CreateVideoRequest {
  title: string
  creator: string
  embedUrl: string
  thumbnailUrl?: string
  durationSeconds?: number
}

export interface UpdateVideoRequest {
  title?: string
  creator?: string
  embedUrl?: string
  thumbnailUrl?: string
  durationSeconds?: number
}

export interface AdminIngredientSubmission {
  id: string
  name: string
  category: IngredientCategory
  description?: string
  status: IngredientSubmissionStatus
  submittedById: string
  submittedByName: string
  submittedAt: string
  reviewedAt?: string
  reviewNotes?: string
}

export interface AdminIngredientSubmissionsResponse {
  submissions: AdminIngredientSubmission[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ApproveSubmissionRequest {
  notes?: string
}

export interface RejectSubmissionRequest {
  reason?: string
}

export interface BatchActionRequest {
  ids: string[]
  notes?: string
}

export interface AdminDashboardCounts {
  featuredRecipes: number
  stories: number
  videos: number
  pendingSubmissions: number
}
