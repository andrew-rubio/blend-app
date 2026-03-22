'use client'

import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { WizardProgress } from './WizardProgress'
import { CuisineStep } from './steps/CuisineStep'
import { DishTypeStep } from './steps/DishTypeStep'
import { DietStep } from './steps/DietStep'
import { IntoleranceStep } from './steps/IntoleranceStep'
import { IngredientsStep } from './steps/IngredientsStep'
import { usePreferenceStore, WIZARD_STEPS } from '@/stores/preferenceStore'
import { useSavePreferences, useCuisines, useDishTypes, useDiets, useIntolerances } from '@/hooks/usePreferences'

export interface PreferenceWizardProps {
  /** Called after a successful save so the parent can navigate or update state. */
  onComplete?: () => void
}

/**
 * Multi-step preference wizard (PREF-01, PREF-05, PREF-11).
 * Supports back navigation, per-step skipping (PREF-17) and submits all
 * selections in a single PUT call on the final step.
 */
export function PreferenceWizard({ onComplete }: PreferenceWizardProps) {
  const router = useRouter()

  const {
    currentStepIndex,
    selections,
    nextStep,
    prevStep,
    toggleCuisine,
    toggleDishType,
    toggleDiet,
    toggleIntolerance,
    addDislikedIngredient,
    removeDislikedIngredient,
    setSavedPreferences,
  } = usePreferenceStore()

  const { data: cuisines = [], isLoading: cuisinesLoading } = useCuisines()
  const { data: dishTypes = [], isLoading: dishTypesLoading } = useDishTypes()
  const { data: diets = [], isLoading: dietsLoading } = useDiets()
  const { data: intolerances = [], isLoading: intolerancesLoading } = useIntolerances()

  const { mutate: savePreferences, isPending, error } = useSavePreferences()

  const isLastStep = currentStepIndex === WIZARD_STEPS.length - 1
  const isFirstStep = currentStepIndex === 0

  function handleSave() {
    savePreferences(
      {
        favoriteCuisines: selections.favoriteCuisines,
        favoriteDishTypes: selections.favoriteDishTypes,
        diets: selections.diets,
        intolerances: selections.intolerances,
        dislikedIngredientIds: selections.dislikedIngredientIds,
      },
      {
        onSuccess: (saved) => {
          setSavedPreferences(saved)
          if (onComplete) {
            onComplete()
          } else {
            router.push('/home')
          }
        },
      }
    )
  }

  function handleSkip() {
    if (isLastStep) {
      handleSave()
    } else {
      nextStep()
    }
  }

  const errorMessage =
    error && typeof error === 'object' && 'message' in error
      ? (error as { message: string }).message
      : error
        ? 'An error occurred while saving your preferences.'
        : null

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <div className="mb-8">
        <h1 className="mb-2 text-center text-2xl font-bold text-gray-900 dark:text-white">
          Set Up Your Preferences
        </h1>
        <p className="text-center text-sm text-gray-500 dark:text-gray-400">
          Help us personalise your recipe experience. Every step is optional — skip anything you
          like.
        </p>
      </div>

      <WizardProgress currentStepIndex={currentStepIndex} />

      <div className="mt-8 rounded-lg border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900">
        {currentStepIndex === 0 && (
          <CuisineStep
            cuisines={cuisines}
            selected={selections.favoriteCuisines}
            onToggle={toggleCuisine}
            isLoading={cuisinesLoading}
          />
        )}
        {currentStepIndex === 1 && (
          <DishTypeStep
            dishTypes={dishTypes}
            selected={selections.favoriteDishTypes}
            onToggle={toggleDishType}
            isLoading={dishTypesLoading}
          />
        )}
        {currentStepIndex === 2 && (
          <DietStep
            diets={diets}
            selected={selections.diets}
            onToggle={toggleDiet}
            isLoading={dietsLoading}
          />
        )}
        {currentStepIndex === 3 && (
          <IntoleranceStep
            intolerances={intolerances}
            selected={selections.intolerances}
            onToggle={toggleIntolerance}
            isLoading={intolerancesLoading}
          />
        )}
        {currentStepIndex === 4 && (
          <IngredientsStep
            addedIds={selections.dislikedIngredientIds}
            onAdd={addDislikedIngredient}
            onRemove={removeDislikedIngredient}
          />
        )}

        {errorMessage && (
          <div
            role="alert"
            className="mt-4 rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300"
          >
            {errorMessage}
          </div>
        )}
      </div>

      <div className="mt-6 flex items-center justify-between gap-3">
        <Button
          variant="ghost"
          onClick={prevStep}
          disabled={isFirstStep || isPending}
          aria-label="Go to previous step"
        >
          ← Back
        </Button>

        <div className="flex gap-3">
          <Button
            variant="outline"
            onClick={handleSkip}
            disabled={isPending}
            aria-label={isLastStep ? 'Skip and save' : 'Skip this step'}
          >
            {isLastStep ? 'Skip & Save' : 'Skip'}
          </Button>

          {isLastStep ? (
            <Button
              variant="primary"
              onClick={handleSave}
              isLoading={isPending}
              aria-label="Save preferences and continue"
            >
          Save & Continue
            </Button>
          ) : (
            <Button
              variant="primary"
              onClick={nextStep}
              disabled={isPending}
              aria-label="Go to next step"
            >
              Next →
            </Button>
          )}
        </div>
      </div>
    </div>
  )
}
