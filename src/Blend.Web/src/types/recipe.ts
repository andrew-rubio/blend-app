export interface Ingredient {
  id: string
  name: string
  amount: number
  unit: string
  originalAmount: number
}

export interface Step {
  number: number
  description: string
  imageUrl?: string
}

export interface Author {
  id: string
  name: string
  avatarUrl?: string
}

export interface Recipe {
  id: string
  title: string
  description: string
  imageUrl?: string
  images?: string[]
  prepTimeMinutes: number
  cookTimeMinutes: number
  totalTimeMinutes: number
  servings: number
  difficulty: 'easy' | 'medium' | 'hard'
  cuisines: string[]
  diets: string[]
  intolerances: string[]
  ingredients: Ingredient[]
  steps: Step[]
  likes: number
  isLiked: boolean
  author: Author
  source: 'community' | 'spoonacular'
  isPrivate?: boolean
  createdAt: string
  updatedAt: string
}
