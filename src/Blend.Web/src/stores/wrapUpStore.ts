import { create } from 'zustand'
import type { PairingFeedbackItem, RecipeDirectionRequest } from '@/types'

// ── Step definitions ──────────────────────────────────────────────────────────

export type WrapUpStep = 'summary' | 'feedback' | 'photos' | 'publish' | 'completion'

export const WRAP_UP_STEPS: WrapUpStep[] = [
  'summary',
  'feedback',
  'photos',
  'publish',
  'completion',
]

export const WRAP_UP_STEP_LABELS: Record<WrapUpStep, string> = {
  summary: 'Session Summary',
  feedback: 'Pairing Feedback',
  photos: 'Photo Upload',
  publish: 'Publish Recipe',
  completion: 'Complete',
}

// ── State shape ───────────────────────────────────────────────────────────────

interface WrapUpState {
  currentStepIndex: number
  /** Pairing feedback collected in step 2. */
  feedbackItems: PairingFeedbackItem[]
  /** Photos uploaded in step 3. */
  photos: string[]
  /** Index of the primary photo in the photos array. */
  primaryPhotoIndex: number
  /** Whether the user chose to publish the session as a recipe. */
  shouldPublish: boolean
  /** Publish form data collected in step 4. */
  publishForm: {
    title: string
    description: string
    directions: RecipeDirectionRequest[]
    cuisineType: string
    tags: string[]
    servings: number
    prepTime: number
    cookTime: number
  }
  /** ID of the published recipe, set after successful publish. */
  publishedRecipeId: string | null
}

interface WrapUpActions {
  nextStep: () => void
  prevStep: () => void
  goToStep: (index: number) => void

  setFeedbackRating: (ingredientId1: string, ingredientId2: string, rating: number, comment?: string) => void
  clearFeedback: () => void

  addPhoto: (url: string) => void
  removePhoto: (index: number) => void
  reorderPhotos: (fromIndex: number, toIndex: number) => void
  setPrimaryPhoto: (index: number) => void

  setShouldPublish: (value: boolean) => void
  setPublishField: <K extends keyof WrapUpState['publishForm']>(
    field: K,
    value: WrapUpState['publishForm'][K],
  ) => void
  addDirection: () => void
  updateDirection: (index: number, text: string) => void
  removeDirection: (index: number) => void

  setPublishedRecipeId: (id: string) => void

  reset: () => void
}

export type WrapUpStore = WrapUpState & WrapUpActions

// ── Initial state ─────────────────────────────────────────────────────────────

const initialState: WrapUpState = {
  currentStepIndex: 0,
  feedbackItems: [],
  photos: [],
  primaryPhotoIndex: 0,
  shouldPublish: false,
  publishForm: {
    title: '',
    description: '',
    directions: [],
    cuisineType: '',
    tags: [],
    servings: 0,
    prepTime: 0,
    cookTime: 0,
  },
  publishedRecipeId: null,
}

// ── Store ─────────────────────────────────────────────────────────────────────

export const useWrapUpStore = create<WrapUpStore>()((set) => ({
  ...initialState,

  nextStep: () =>
    set((state: WrapUpStore) => ({
      currentStepIndex: Math.min(state.currentStepIndex + 1, WRAP_UP_STEPS.length - 1),
    })),

  prevStep: () =>
    set((state: WrapUpStore) => ({ currentStepIndex: Math.max(state.currentStepIndex - 1, 0) })),

  goToStep: (index) =>
    set({ currentStepIndex: Math.max(0, Math.min(index, WRAP_UP_STEPS.length - 1)) }),

  setFeedbackRating: (ingredientId1: string, ingredientId2: string, rating: number, comment?: string) =>
    set((state: WrapUpStore) => {
      const existing = state.feedbackItems.findIndex(
        (f: PairingFeedbackItem) => f.ingredientId1 === ingredientId1 && f.ingredientId2 === ingredientId2,
      )
      if (existing >= 0) {
        const updated = [...state.feedbackItems]
        updated[existing] = { ingredientId1, ingredientId2, rating, comment }
        return { feedbackItems: updated }
      }
      return { feedbackItems: [...state.feedbackItems, { ingredientId1, ingredientId2, rating, comment }] }
    }),

  clearFeedback: () => set({ feedbackItems: [] }),

  addPhoto: (url: string) =>
    set((state: WrapUpStore) => {
      if (state.photos.length >= 5) return {}
      return { photos: [...state.photos, url] }
    }),

  removePhoto: (index: number) =>
    set((state: WrapUpStore) => {
      const updated = state.photos.filter((_: string, i: number) => i !== index)
      return {
        photos: updated,
        primaryPhotoIndex: Math.min(state.primaryPhotoIndex, Math.max(0, updated.length - 1)),
      }
    }),

  reorderPhotos: (fromIndex: number, toIndex: number) =>
    set((state: WrapUpStore) => {
      const updated = [...state.photos]
      const [moved] = updated.splice(fromIndex, 1)
      updated.splice(toIndex, 0, moved)
      return { photos: updated }
    }),

  setPrimaryPhoto: (index: number) => set({ primaryPhotoIndex: index }),

  setShouldPublish: (value: boolean) => set({ shouldPublish: value }),

  setPublishField: <K extends keyof WrapUpState['publishForm']>(field: K, value: WrapUpState['publishForm'][K]) =>
    set((state: WrapUpStore) => ({
      publishForm: { ...state.publishForm, [field]: value },
    })),

  addDirection: () =>
    set((state: WrapUpStore) => {
      const next = state.publishForm.directions.length + 1
      return {
        publishForm: {
          ...state.publishForm,
          directions: [...state.publishForm.directions, { stepNumber: next, text: '' }],
        },
      }
    }),

  updateDirection: (index: number, text: string) =>
    set((state: WrapUpStore) => {
      const updated = [...state.publishForm.directions]
      updated[index] = { ...updated[index], text }
      return { publishForm: { ...state.publishForm, directions: updated } }
    }),

  removeDirection: (index: number) =>
    set((state: WrapUpStore) => {
      const updated = state.publishForm.directions
        .filter((_: RecipeDirectionRequest, i: number) => i !== index)
        .map((d: RecipeDirectionRequest, i: number) => ({ ...d, stepNumber: i + 1 }))
      return { publishForm: { ...state.publishForm, directions: updated } }
    }),

  setPublishedRecipeId: (id: string) => set({ publishedRecipeId: id }),

  reset: () => set(initialState),
}))
