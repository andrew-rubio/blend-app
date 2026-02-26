import { create } from 'zustand';
import type { UserPreferences, Ingredient } from '@/types/preferences';

interface PreferencesState {
  preferences: UserPreferences;
  dislikedIngredients: Ingredient[];
  setFavoriteCuisines: (cuisines: string[]) => void;
  setFavoriteDishTypes: (types: string[]) => void;
  setDiets: (diets: string[]) => void;
  setIntolerances: (intolerances: string[]) => void;
  addDislikedIngredient: (ingredient: Ingredient) => void;
  removeDislikedIngredient: (id: string) => void;
  setPreferences: (prefs: UserPreferences) => void;
  reset: () => void;
}

const initialPreferences: UserPreferences = {
  favoriteCuisines: [],
  favoriteDishTypes: [],
  diets: [],
  intolerances: [],
  dislikedIngredientIds: [],
};

export const usePreferencesStore = create<PreferencesState>((set) => ({
  preferences: initialPreferences,
  dislikedIngredients: [],
  setFavoriteCuisines: (cuisines) =>
    set((s) => ({ preferences: { ...s.preferences, favoriteCuisines: cuisines } })),
  setFavoriteDishTypes: (types) =>
    set((s) => ({ preferences: { ...s.preferences, favoriteDishTypes: types } })),
  setDiets: (diets) =>
    set((s) => ({ preferences: { ...s.preferences, diets } })),
  setIntolerances: (intolerances) =>
    set((s) => ({ preferences: { ...s.preferences, intolerances } })),
  addDislikedIngredient: (ingredient) =>
    set((s) => ({
      dislikedIngredients: [...s.dislikedIngredients.filter((i) => i.id !== ingredient.id), ingredient],
      preferences: {
        ...s.preferences,
        dislikedIngredientIds: [
          ...s.preferences.dislikedIngredientIds.filter((id) => id !== ingredient.id),
          ingredient.id,
        ],
      },
    })),
  removeDislikedIngredient: (id) =>
    set((s) => ({
      dislikedIngredients: s.dislikedIngredients.filter((i) => i.id !== id),
      preferences: {
        ...s.preferences,
        dislikedIngredientIds: s.preferences.dislikedIngredientIds.filter((i) => i !== id),
      },
    })),
  setPreferences: (prefs) => set({ preferences: prefs }),
  reset: () => set({ preferences: initialPreferences, dislikedIngredients: [] }),
}));
