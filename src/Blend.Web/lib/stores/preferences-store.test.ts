import { usePreferencesStore } from './preferences-store';

describe('usePreferencesStore', () => {
  beforeEach(() => {
    usePreferencesStore.getState().reset();
  });

  it('has empty initial preferences', () => {
    const { preferences } = usePreferencesStore.getState();
    expect(preferences.favoriteCuisines).toEqual([]);
    expect(preferences.favoriteDishTypes).toEqual([]);
    expect(preferences.diets).toEqual([]);
    expect(preferences.intolerances).toEqual([]);
    expect(preferences.dislikedIngredientIds).toEqual([]);
  });

  it('setFavoriteCuisines updates cuisines', () => {
    usePreferencesStore.getState().setFavoriteCuisines(['Italian', 'Asian']);
    expect(usePreferencesStore.getState().preferences.favoriteCuisines).toEqual(['Italian', 'Asian']);
  });

  it('setFavoriteDishTypes updates dish types', () => {
    usePreferencesStore.getState().setFavoriteDishTypes(['Soup', 'Dessert']);
    expect(usePreferencesStore.getState().preferences.favoriteDishTypes).toEqual(['Soup', 'Dessert']);
  });

  it('setDiets updates diets', () => {
    usePreferencesStore.getState().setDiets(['vegan', 'gluten free']);
    expect(usePreferencesStore.getState().preferences.diets).toEqual(['vegan', 'gluten free']);
  });

  it('setIntolerances updates intolerances', () => {
    usePreferencesStore.getState().setIntolerances(['dairy', 'egg']);
    expect(usePreferencesStore.getState().preferences.intolerances).toEqual(['dairy', 'egg']);
  });

  it('addDislikedIngredient adds ingredient and id', () => {
    usePreferencesStore.getState().addDislikedIngredient({ id: '42', name: 'Cilantro' });
    const state = usePreferencesStore.getState();
    expect(state.dislikedIngredients).toEqual([{ id: '42', name: 'Cilantro' }]);
    expect(state.preferences.dislikedIngredientIds).toEqual(['42']);
  });

  it('addDislikedIngredient does not duplicate', () => {
    usePreferencesStore.getState().addDislikedIngredient({ id: '42', name: 'Cilantro' });
    usePreferencesStore.getState().addDislikedIngredient({ id: '42', name: 'Cilantro' });
    expect(usePreferencesStore.getState().dislikedIngredients).toHaveLength(1);
  });

  it('removeDislikedIngredient removes ingredient and id', () => {
    usePreferencesStore.getState().addDislikedIngredient({ id: '42', name: 'Cilantro' });
    usePreferencesStore.getState().removeDislikedIngredient('42');
    const state = usePreferencesStore.getState();
    expect(state.dislikedIngredients).toEqual([]);
    expect(state.preferences.dislikedIngredientIds).toEqual([]);
  });

  it('setPreferences replaces preferences', () => {
    const newPrefs = {
      favoriteCuisines: ['French'],
      favoriteDishTypes: ['Soup'],
      diets: ['vegan'],
      intolerances: ['gluten'],
      dislikedIngredientIds: ['1'],
    };
    usePreferencesStore.getState().setPreferences(newPrefs);
    expect(usePreferencesStore.getState().preferences).toEqual(newPrefs);
  });

  it('reset clears all state', () => {
    usePreferencesStore.getState().setFavoriteCuisines(['Italian']);
    usePreferencesStore.getState().addDislikedIngredient({ id: '1', name: 'Garlic' });
    usePreferencesStore.getState().reset();
    const state = usePreferencesStore.getState();
    expect(state.preferences.favoriteCuisines).toEqual([]);
    expect(state.dislikedIngredients).toEqual([]);
  });
});
