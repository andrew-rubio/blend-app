'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { WizardProgress } from '@/components/ui/WizardProgress';
import { SelectionChip } from '@/components/ui/SelectionChip';
import { IngredientTypeahead } from '@/components/features/IngredientTypeahead';
import { usePreferencesStore } from '@/lib/stores/preferences-store';
import { useSavePreferences } from '@/hooks/use-preferences';
import { CUISINES, DISH_TYPES, DIETS, INTOLERANCES } from '@/types/preferences';

const STEP_LABELS = ['Cuisines', 'Dish Types', 'Diets', 'Intolerances', 'Ingredients'];
const TOTAL_STEPS = 5;

export default function OnboardingPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState(1);
  const {
    preferences,
    dislikedIngredients,
    setFavoriteCuisines,
    setFavoriteDishTypes,
    setDiets,
    setIntolerances,
    addDislikedIngredient,
    removeDislikedIngredient,
  } = usePreferencesStore();
  const { mutate: savePreferences, isPending, error } = useSavePreferences();

  function toggleItem(list: string[], item: string, setter: (items: string[]) => void) {
    if (list.includes(item)) {
      setter(list.filter((i) => i !== item));
    } else {
      setter([...list, item]);
    }
  }

  function handleNext() {
    if (currentStep < TOTAL_STEPS) {
      setCurrentStep((s) => s + 1);
    } else {
      handleSave();
    }
  }

  function handleBack() {
    if (currentStep > 1) {
      setCurrentStep((s) => s - 1);
    }
  }

  function handleSkip() {
    if (currentStep < TOTAL_STEPS) {
      setCurrentStep((s) => s + 1);
    } else {
      router.push('/');
    }
  }

  function handleSave() {
    savePreferences(preferences, {
      onSuccess: () => router.push('/'),
    });
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
      <div className="w-full max-w-2xl bg-white rounded-2xl shadow-lg p-6 space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Set Up Your Preferences</h1>
          <p className="text-gray-500 text-sm mt-1">Tell us what you like so we can personalise your experience.</p>
        </div>

        <WizardProgress currentStep={currentStep} totalSteps={TOTAL_STEPS} labels={STEP_LABELS} />

        <div className="min-h-[250px]">
          {currentStep === 1 && (
            <div>
              <h2 className="text-lg font-semibold mb-3">Favourite Cuisines</h2>
              <p className="text-sm text-gray-500 mb-4">Select the cuisines you enjoy most.</p>
              <div className="flex flex-wrap gap-2">
                {CUISINES.map((cuisine) => (
                  <SelectionChip
                    key={cuisine}
                    label={cuisine}
                    selected={preferences.favoriteCuisines.includes(cuisine)}
                    onToggle={() => toggleItem(preferences.favoriteCuisines, cuisine, setFavoriteCuisines)}
                  />
                ))}
              </div>
            </div>
          )}

          {currentStep === 2 && (
            <div>
              <h2 className="text-lg font-semibold mb-3">Favourite Dish Types</h2>
              <p className="text-sm text-gray-500 mb-4">What kinds of dishes do you like to cook?</p>
              <div className="flex flex-wrap gap-2">
                {DISH_TYPES.map((type) => (
                  <SelectionChip
                    key={type}
                    label={type}
                    selected={preferences.favoriteDishTypes.includes(type)}
                    onToggle={() => toggleItem(preferences.favoriteDishTypes, type, setFavoriteDishTypes)}
                  />
                ))}
              </div>
            </div>
          )}

          {currentStep === 3 && (
            <div>
              <h2 className="text-lg font-semibold mb-3">Dietary Preferences</h2>
              <p className="text-sm text-gray-500 mb-4">Select any dietary plans that apply to you.</p>
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
                      onChange={() => toggleItem(preferences.diets, diet.value, setDiets)}
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
            </div>
          )}

          {currentStep === 4 && (
            <div>
              <h2 className="text-lg font-semibold mb-1">Intolerances</h2>
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
                    onToggle={() => toggleItem(preferences.intolerances, intolerance.value, setIntolerances)}
                  />
                ))}
              </div>
            </div>
          )}

          {currentStep === 5 && (
            <div>
              <h2 className="text-lg font-semibold mb-3">Disliked Ingredients</h2>
              <p className="text-sm text-gray-500 mb-4">Search for ingredients you{"'"}d like to avoid.</p>
              <IngredientTypeahead
                selectedIngredients={dislikedIngredients}
                onAdd={addDislikedIngredient}
                onRemove={removeDislikedIngredient}
              />
            </div>
          )}
        </div>

        {error && (
          <p className="text-red-600 text-sm" role="alert">
            Failed to save preferences. Please try again.
          </p>
        )}

        <div className="flex items-center justify-between pt-4 border-t">
          <button
            type="button"
            onClick={handleBack}
            disabled={currentStep === 1}
            className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900 disabled:opacity-30 disabled:cursor-not-allowed"
            aria-label="Go to previous step"
          >
            ← Back
          </button>
          <button
            type="button"
            onClick={handleSkip}
            className="px-4 py-2 text-sm text-gray-400 hover:text-gray-600"
          >
            Skip
          </button>
          <button
            type="button"
            onClick={handleNext}
            disabled={isPending}
            className="px-6 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isPending ? 'Saving...' : currentStep === TOTAL_STEPS ? 'Save & Continue' : 'Next →'}
          </button>
        </div>
      </div>
    </div>
  );
}
