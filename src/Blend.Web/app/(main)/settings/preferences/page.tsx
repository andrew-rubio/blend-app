'use client';
import { useEffect } from 'react';
import { SelectionChip } from '@/components/ui/SelectionChip';
import { IngredientTypeahead } from '@/components/features/IngredientTypeahead';
import { usePreferences, useSavePreferences } from '@/hooks/use-preferences';
import { usePreferencesStore } from '@/lib/stores/preferences-store';
import { CUISINES, DISH_TYPES, DIETS, INTOLERANCES } from '@/types/preferences';

export default function ManagePreferencesPage() {
  const { data: savedPrefs, isLoading, isError } = usePreferences();
  const { mutate: savePreferences, isPending, isSuccess } = useSavePreferences();
  const {
    preferences,
    dislikedIngredients,
    setPreferences,
    addDislikedIngredient,
    removeDislikedIngredient,
  } = usePreferencesStore();

  useEffect(() => {
    if (savedPrefs) {
      setPreferences(savedPrefs);
    }
  }, [savedPrefs, setPreferences]);

  function toggleItem(list: string[], item: string, setter: (items: string[]) => void) {
    if (list.includes(item)) {
      setter(list.filter((i) => i !== item));
    } else {
      setter([...list, item]);
    }
  }

  function handleSave() {
    savePreferences(preferences);
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-64" aria-label="Loading preferences">
        <div className="w-8 h-8 border-4 border-primary border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-6 text-center" role="alert">
        <p className="text-red-600">Failed to load preferences. Please try again.</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto p-6 space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Manage Preferences</h1>
        <p className="text-gray-500 text-sm mt-1">Customise your recipe experience.</p>
      </div>

      {/* Cuisines */}
      <section aria-labelledby="cuisines-heading">
        <h2 id="cuisines-heading" className="text-lg font-semibold mb-3">Favourite Cuisines</h2>
        <div className="flex flex-wrap gap-2">
          {CUISINES.map((cuisine) => (
            <SelectionChip
              key={cuisine}
              label={cuisine}
              selected={preferences.favoriteCuisines.includes(cuisine)}
              onToggle={() =>
                toggleItem(preferences.favoriteCuisines, cuisine, (items) =>
                  setPreferences({ ...preferences, favoriteCuisines: items })
                )
              }
            />
          ))}
        </div>
      </section>

      {/* Dish Types */}
      <section aria-labelledby="dish-types-heading">
        <h2 id="dish-types-heading" className="text-lg font-semibold mb-3">Favourite Dish Types</h2>
        <div className="flex flex-wrap gap-2">
          {DISH_TYPES.map((type) => (
            <SelectionChip
              key={type}
              label={type}
              selected={preferences.favoriteDishTypes.includes(type)}
              onToggle={() =>
                toggleItem(preferences.favoriteDishTypes, type, (items) =>
                  setPreferences({ ...preferences, favoriteDishTypes: items })
                )
              }
            />
          ))}
        </div>
      </section>

      {/* Diets */}
      <section aria-labelledby="diets-heading">
        <h2 id="diets-heading" className="text-lg font-semibold mb-3">Dietary Preferences</h2>
        <div className="space-y-2">
          {DIETS.map((diet) => (
            <label
              key={diet.value}
              className={`flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                preferences.diets.includes(diet.value)
                  ? 'border-primary bg-primary/5'
                  : 'border-gray-200 hover:border-gray-300'
              }`}
            >
              <input
                type="checkbox"
                className="sr-only"
                checked={preferences.diets.includes(diet.value)}
                onChange={() =>
                  toggleItem(preferences.diets, diet.value, (items) =>
                    setPreferences({ ...preferences, diets: items })
                  )
                }
                aria-label={diet.label}
              />
              <div
                className={`w-5 h-5 rounded border-2 flex items-center justify-center flex-shrink-0 ${
                  preferences.diets.includes(diet.value) ? 'border-primary bg-primary' : 'border-gray-300'
                }`}
              >
                {preferences.diets.includes(diet.value) && (
                  <svg className="w-3 h-3 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                  </svg>
                )}
              </div>
              <div>
                <div className="font-medium text-sm">{diet.label}</div>
                <div className="text-xs text-gray-500">{diet.description}</div>
              </div>
            </label>
          ))}
        </div>
      </section>

      {/* Intolerances */}
      <section aria-labelledby="intolerances-heading">
        <h2 id="intolerances-heading" className="text-lg font-semibold mb-1">Intolerances</h2>
        <div className="flex items-start gap-2 p-3 bg-amber-50 border border-amber-200 rounded-lg mb-4">
          <svg
            className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
          <p className="text-sm text-amber-800">
            <strong>Strict exclusion:</strong> Recipes containing these ingredients will never appear in your results.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          {INTOLERANCES.map((intolerance) => (
            <SelectionChip
              key={intolerance.value}
              label={intolerance.label}
              selected={preferences.intolerances.includes(intolerance.value)}
              onToggle={() =>
                toggleItem(preferences.intolerances, intolerance.value, (items) =>
                  setPreferences({ ...preferences, intolerances: items })
                )
              }
            />
          ))}
        </div>
      </section>

      {/* Disliked Ingredients */}
      <section aria-labelledby="disliked-heading">
        <h2 id="disliked-heading" className="text-lg font-semibold mb-3">Disliked Ingredients</h2>
        <IngredientTypeahead
          selectedIngredients={dislikedIngredients}
          onAdd={addDislikedIngredient}
          onRemove={removeDislikedIngredient}
        />
      </section>

      {isSuccess && (
        <p className="text-green-600 text-sm" role="status">
          Preferences saved successfully!
        </p>
      )}

      <div className="flex justify-end pt-4 border-t">
        <button
          type="button"
          onClick={handleSave}
          disabled={isPending}
          className="px-6 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isPending ? 'Saving...' : 'Save Preferences'}
        </button>
      </div>
    </div>
  );
}
