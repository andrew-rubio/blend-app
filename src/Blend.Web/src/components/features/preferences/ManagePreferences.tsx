'use client'

import { useEffect, useState } from 'react'
import { Button } from '@/components/ui/Button'
import { SelectionChip } from './SelectionChip'
import { IngredientTypeahead } from './IngredientTypeahead'
import { useUserPreferences, useSavePreferences, useCuisines, useDishTypes, useDiets, useIntolerances } from '@/hooks/usePreferences'
import type { UpdatePreferencesRequest } from '@/types'

/**
 * Manage Preferences screen — shown in App Settings (PREF-15).
 * Pre-populates from the API, allows editing and saving.
 * Shows confirmation before clearing all preferences.
 */
export function ManagePreferences() {
  const { data: saved, isLoading: loadingPrefs, error: prefsError } = useUserPreferences()
  const { data: cuisines = [], isLoading: cuisinesLoading } = useCuisines()
  const { data: dishTypes = [], isLoading: dishTypesLoading } = useDishTypes()
  const { data: diets = [], isLoading: dietsLoading } = useDiets()
  const { data: intolerances = [], isLoading: intolerancesLoading } = useIntolerances()

  const { mutate: savePreferences, isPending, error: saveError, isSuccess } = useSavePreferences()

  const [favoriteCuisines, setFavoriteCuisines] = useState<string[]>([])
  const [favoriteDishTypes, setFavoriteDishTypes] = useState<string[]>([])
  const [selectedDiets, setSelectedDiets] = useState<string[]>([])
  const [selectedIntolerances, setSelectedIntolerances] = useState<string[]>([])
  const [dislikedIngredientIds, setDislikedIngredientIds] = useState<string[]>([])
  const [showClearConfirm, setShowClearConfirm] = useState(false)

  // Pre-populate form from saved preferences
  useEffect(() => {
    if (saved) {
      setFavoriteCuisines([...saved.favoriteCuisines])
      setFavoriteDishTypes([...saved.favoriteDishTypes])
      setSelectedDiets([...saved.diets])
      setSelectedIntolerances([...saved.intolerances])
      setDislikedIngredientIds([...saved.dislikedIngredientIds])
    }
  }, [saved])

  function toggle<T extends string>(
    value: T,
    current: T[],
    setter: (v: T[]) => void
  ) {
    if (current.includes(value)) {
      setter(current.filter((v) => v !== value))
    } else {
      setter([...current, value])
    }
  }

  function handleSave() {
    const payload: UpdatePreferencesRequest = {
      favoriteCuisines,
      favoriteDishTypes,
      diets: selectedDiets,
      intolerances: selectedIntolerances,
      dislikedIngredientIds,
    }
    savePreferences(payload)
  }

  function handleClearAll() {
    setShowClearConfirm(true)
  }

  function confirmClearAll() {
    const payload: UpdatePreferencesRequest = {
      favoriteCuisines: [],
      favoriteDishTypes: [],
      diets: [],
      intolerances: [],
      dislikedIngredientIds: [],
    }
    savePreferences(payload, {
      onSuccess: () => {
        setFavoriteCuisines([])
        setFavoriteDishTypes([])
        setSelectedDiets([])
        setSelectedIntolerances([])
        setDislikedIngredientIds([])
        setShowClearConfirm(false)
      },
    })
  }

  const saveErrorMessage =
    saveError && typeof saveError === 'object' && 'message' in saveError
      ? (saveError as { message: string }).message
      : saveError
        ? 'Failed to save preferences.'
        : null

  const isLoading = loadingPrefs || cuisinesLoading || dishTypesLoading || dietsLoading || intolerancesLoading

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-16" aria-live="polite" aria-busy="true">
        <span className="h-6 w-6 animate-spin rounded-full border-2 border-primary-600 border-t-transparent" />
        <span className="ml-2 text-sm text-gray-500 dark:text-gray-400">Loading preferences…</span>
      </div>
    )
  }

  if (prefsError) {
    return (
      <div
        role="alert"
        className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300"
      >
        Failed to load preferences. Please try again later.
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-2xl space-y-8 py-4">
      {isSuccess && (
        <div
          role="status"
          aria-live="polite"
          className="rounded-md border border-green-300 bg-green-50 px-4 py-3 text-sm text-green-700 dark:border-green-700 dark:bg-green-900/30 dark:text-green-300"
        >
          Preferences saved successfully!
        </div>
      )}

      {saveErrorMessage && (
        <div
          role="alert"
          className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300"
        >
          {saveErrorMessage}
        </div>
      )}

      {/* Cuisines */}
      <section aria-labelledby="cuisines-heading">
        <h2 id="cuisines-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
          Favorite Cuisines
        </h2>
        <div role="group" aria-label="Cuisine selection" className="flex flex-wrap gap-2">
          {cuisines.map((c) => (
            <SelectionChip
              key={c}
              label={c}
              selected={favoriteCuisines.includes(c)}
              onClick={() => toggle(c, favoriteCuisines, setFavoriteCuisines)}
              disabled={isPending}
            />
          ))}
        </div>
      </section>

      {/* Dish Types */}
      <section aria-labelledby="dish-types-heading">
        <h2 id="dish-types-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
          Dish Types
        </h2>
        <div role="group" aria-label="Dish type selection" className="flex flex-wrap gap-2">
          {dishTypes.map((d) => (
            <SelectionChip
              key={d}
              label={d}
              selected={favoriteDishTypes.includes(d)}
              onClick={() => toggle(d, favoriteDishTypes, setFavoriteDishTypes)}
              disabled={isPending}
            />
          ))}
        </div>
      </section>

      {/* Diets */}
      <section aria-labelledby="diets-heading">
        <h2 id="diets-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
          Dietary Preferences
        </h2>
        <div role="group" aria-label="Dietary preference selection" className="flex flex-wrap gap-2">
          {diets.map((d) => (
            <SelectionChip
              key={d}
              label={d}
              selected={selectedDiets.includes(d)}
              onClick={() => toggle(d, selectedDiets, setSelectedDiets)}
              disabled={isPending}
            />
          ))}
        </div>
      </section>

      {/* Intolerances */}
      <section aria-labelledby="intolerances-heading">
        <h2 id="intolerances-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
          Intolerances
        </h2>
        <p className="mb-2 text-sm text-amber-700 dark:text-amber-300">
          ⚠ These will strictly exclude any recipe containing the selected allergens.
        </p>
        <div role="group" aria-label="Intolerance selection" className="flex flex-wrap gap-2">
          {intolerances.map((i) => (
            <SelectionChip
              key={i}
              label={i}
              selected={selectedIntolerances.includes(i)}
              onClick={() => toggle(i, selectedIntolerances, setSelectedIntolerances)}
              disabled={isPending}
            />
          ))}
        </div>
      </section>

      {/* Disliked Ingredients */}
      <section aria-labelledby="ingredients-heading">
        <h2 id="ingredients-heading" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
          Disliked Ingredients
        </h2>
        <IngredientTypeahead
          addedIds={dislikedIngredientIds}
          onAdd={(id) => setDislikedIngredientIds((prev) => [...prev, id])}
          onRemove={(id) => setDislikedIngredientIds((prev) => prev.filter((i) => i !== id))}
          disabled={isPending}
        />
      </section>

      {/* Actions */}
      <div className="flex items-center justify-between border-t border-gray-200 pt-6 dark:border-gray-800">
        <Button
          variant="destructive"
          size="sm"
          onClick={handleClearAll}
          disabled={isPending}
          aria-label="Clear all preferences"
        >
          Clear All
        </Button>
        <Button
          variant="primary"
          onClick={handleSave}
          isLoading={isPending}
          aria-label="Save preferences"
        >
          Save Preferences
        </Button>
      </div>

      {/* Clear all confirmation dialog */}
      {showClearConfirm && (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="clear-confirm-title"
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
        >
          <div className="mx-4 max-w-sm rounded-lg bg-white p-6 shadow-xl dark:bg-gray-900">
            <h3 id="clear-confirm-title" className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">
              Clear All Preferences?
            </h3>
            <p className="mb-5 text-sm text-gray-600 dark:text-gray-400">
              This will remove all your saved cuisines, dish types, diets, intolerances, and
              disliked ingredients. This action cannot be undone.
            </p>
            <div className="flex justify-end gap-3">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowClearConfirm(false)}
                disabled={isPending}
              >
                Cancel
              </Button>
              <Button
                variant="destructive"
                size="sm"
                onClick={confirmClearAll}
                isLoading={isPending}
              >
                Yes, Clear All
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
