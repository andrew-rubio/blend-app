'use client'

import { useState } from 'react'
import type { Recipe } from '@/types'
import type { UpdateRecipePayload } from '@/lib/api/recipes'

interface RecipeIngredientFormItem {
  ingredientId?: string
  ingredientName: string
  quantity: number
  unit: string
}

interface RecipeDirectionFormItem {
  stepNumber: number
  text: string
}

interface RecipeEditFormProps {
  recipe: Recipe
  onSave: (payload: UpdateRecipePayload) => void
  onCancel: () => void
  isSaving?: boolean
  saveError?: string | null
}

/**
 * Re-usable recipe edit form (REQ-60).
 * Pre-populated with current recipe data. Identical to the publish form structure.
 * Only the recipe owner should be able to render this form.
 */
export function RecipeEditForm({
  recipe,
  onSave,
  onCancel,
  isSaving = false,
  saveError,
}: RecipeEditFormProps) {
  const [title, setTitle] = useState(recipe.title)
  const [description, setDescription] = useState(recipe.description ?? '')
  const [cuisineType, setCuisineType] = useState(recipe.cuisines[0] ?? '')
  const [servings, setServings] = useState(recipe.servings ?? 0)
  const [prepTime, setPrepTime] = useState(recipe.prepTimeMinutes ?? 0)
  const [cookTime, setCookTime] = useState(recipe.cookTimeMinutes ?? 0)
  const [isPublic, setIsPublic] = useState(true)

  const [ingredients, setIngredients] = useState<RecipeIngredientFormItem[]>(() =>
    recipe.ingredients.map((i) => ({
      ingredientId: i.id,
      ingredientName: i.name,
      quantity: i.amount,
      unit: i.unit,
    })),
  )

  const [directions, setDirections] = useState<RecipeDirectionFormItem[]>(() =>
    recipe.steps.map((s) => ({ stepNumber: s.number, text: s.step })),
  )

  const [titleError, setTitleError] = useState<string | null>(null)
  const [directionsError, setDirectionsError] = useState<string | null>(null)

  function addIngredient() {
    setIngredients((prev) => [...prev, { ingredientName: '', quantity: 1, unit: '' }])
  }

  function updateIngredient(index: number, field: keyof RecipeIngredientFormItem, value: string | number) {
    setIngredients((prev) => {
      const updated = [...prev]
      updated[index] = { ...updated[index], [field]: value }
      return updated
    })
  }

  function removeIngredient(index: number) {
    setIngredients((prev) => prev.filter((_, i) => i !== index))
  }

  function addDirection() {
    setDirections((prev) => [
      ...prev,
      { stepNumber: prev.length + 1, text: '' },
    ])
  }

  function updateDirection(index: number, text: string) {
    setDirections((prev) => {
      const updated = [...prev]
      updated[index] = { ...updated[index], text }
      return updated
    })
  }

  function removeDirection(index: number) {
    setDirections((prev) =>
      prev
        .filter((_, i) => i !== index)
        .map((d, i) => ({ ...d, stepNumber: i + 1 })),
    )
  }

  function validate(): boolean {
    let valid = true
    if (!title.trim()) {
      setTitleError('Title is required.')
      valid = false
    } else {
      setTitleError(null)
    }
    if (isPublic && directions.length === 0) {
      setDirectionsError('At least one direction step is required for public recipes.')
      valid = false
    } else {
      setDirectionsError(null)
    }
    return valid
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return

    const payload: UpdateRecipePayload = {
      title: title.trim(),
      description: description.trim() || undefined,
      ingredients: ingredients
        .filter((i) => i.ingredientName.trim())
        .map((i) => ({
          ingredientId: i.ingredientId,
          ingredientName: i.ingredientName.trim(),
          quantity: i.quantity,
          unit: i.unit.trim(),
        })),
      directions: directions.filter((d) => d.text.trim()),
      prepTime,
      cookTime,
      servings,
      cuisineType: cuisineType.trim() || undefined,
      isPublic,
    }
    onSave(payload)
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-6"
      data-testid="recipe-edit-form"
      aria-label="Edit recipe"
      noValidate
    >
      {/* Title */}
      <div>
        <label
          htmlFor="edit-title"
          className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
        >
          Title <span className="text-red-500" aria-hidden="true">*</span>
        </label>
        <input
          id="edit-title"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          maxLength={200}
          className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
          aria-required="true"
          aria-describedby={titleError ? 'edit-title-error' : undefined}
          data-testid="edit-title-input"
        />
        {titleError && (
          <p id="edit-title-error" className="mt-1 text-xs text-red-500" role="alert">
            {titleError}
          </p>
        )}
      </div>

      {/* Description */}
      <div>
        <label
          htmlFor="edit-description"
          className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
        >
          Description
        </label>
        <textarea
          id="edit-description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
          data-testid="edit-description-input"
        />
      </div>

      {/* Ingredients */}
      <div>
        <div className="mb-2 flex items-center justify-between">
          <p className="text-sm font-medium text-gray-700 dark:text-gray-300">Ingredients</p>
          <button
            type="button"
            onClick={addIngredient}
            className="text-sm text-primary-600 hover:underline dark:text-primary-400"
            aria-label="Add ingredient"
            data-testid="add-ingredient-button"
          >
            + Add ingredient
          </button>
        </div>
        <div className="space-y-2" data-testid="ingredients-list">
          {ingredients.map((ing, index) => (
            <div key={index} className="flex items-center gap-2">
              <input
                type="text"
                value={ing.ingredientName}
                onChange={(e) => updateIngredient(index, 'ingredientName', e.target.value)}
                placeholder="Ingredient name"
                className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                aria-label={`Ingredient ${index + 1} name`}
                data-testid={`ingredient-name-${index}`}
              />
              <input
                type="number"
                value={ing.quantity || ''}
                onChange={(e) => updateIngredient(index, 'quantity', Number(e.target.value))}
                placeholder="Qty"
                min={0}
                className="w-20 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                aria-label={`Ingredient ${index + 1} quantity`}
                data-testid={`ingredient-qty-${index}`}
              />
              <input
                type="text"
                value={ing.unit}
                onChange={(e) => updateIngredient(index, 'unit', e.target.value)}
                placeholder="Unit"
                className="w-20 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                aria-label={`Ingredient ${index + 1} unit`}
                data-testid={`ingredient-unit-${index}`}
              />
              <button
                type="button"
                onClick={() => removeIngredient(index)}
                className="text-sm text-red-500 hover:text-red-700"
                aria-label={`Remove ingredient ${index + 1}`}
                data-testid={`remove-ingredient-${index}`}
              >
                ✕
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* Directions */}
      <div>
        <div className="mb-2 flex items-center justify-between">
          <p className="text-sm font-medium text-gray-700 dark:text-gray-300">
            Directions{' '}
            {isPublic && <span className="text-red-500" aria-hidden="true">*</span>}
          </p>
          <button
            type="button"
            onClick={addDirection}
            className="text-sm text-primary-600 hover:underline dark:text-primary-400"
            aria-label="Add direction step"
            data-testid="add-direction-button"
          >
            + Add step
          </button>
        </div>
        <ol className="space-y-2" data-testid="directions-list">
          {directions.map((dir, index) => (
            <li key={index} className="flex items-center gap-2">
              <span className="min-w-[1.5rem] text-sm font-medium text-gray-500 dark:text-gray-400">
                {dir.stepNumber}.
              </span>
              <input
                type="text"
                value={dir.text}
                onChange={(e) => updateDirection(index, e.target.value)}
                placeholder={`Step ${dir.stepNumber}`}
                className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                aria-label={`Step ${dir.stepNumber}`}
                data-testid={`direction-input-${index}`}
              />
              <button
                type="button"
                onClick={() => removeDirection(index)}
                className="text-sm text-red-500 hover:text-red-700"
                aria-label={`Remove step ${dir.stepNumber}`}
                data-testid={`remove-direction-${index}`}
              >
                ✕
              </button>
            </li>
          ))}
        </ol>
        {directionsError && (
          <p className="mt-1 text-xs text-red-500" role="alert" data-testid="directions-error">
            {directionsError}
          </p>
        )}
      </div>

      {/* Optional fields */}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        <div>
          <label
            htmlFor="edit-cuisine"
            className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
          >
            Cuisine type
          </label>
          <input
            id="edit-cuisine"
            type="text"
            value={cuisineType}
            onChange={(e) => setCuisineType(e.target.value)}
            placeholder="e.g. Italian"
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
            data-testid="edit-cuisine-input"
          />
        </div>
        <div>
          <label
            htmlFor="edit-servings"
            className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
          >
            Servings
          </label>
          <input
            id="edit-servings"
            type="number"
            min={0}
            value={servings || ''}
            onChange={(e) => setServings(Number(e.target.value))}
            placeholder="2"
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
            data-testid="edit-servings-input"
          />
        </div>
        <div>
          <label
            htmlFor="edit-prep-time"
            className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
          >
            Prep time (min)
          </label>
          <input
            id="edit-prep-time"
            type="number"
            min={0}
            value={prepTime || ''}
            onChange={(e) => setPrepTime(Number(e.target.value))}
            placeholder="15"
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
            data-testid="edit-prep-time-input"
          />
        </div>
        <div>
          <label
            htmlFor="edit-cook-time"
            className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
          >
            Cook time (min)
          </label>
          <input
            id="edit-cook-time"
            type="number"
            min={0}
            value={cookTime || ''}
            onChange={(e) => setCookTime(Number(e.target.value))}
            placeholder="30"
            className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none dark:border-gray-600 dark:bg-gray-800 dark:text-white"
            data-testid="edit-cook-time-input"
          />
        </div>
      </div>

      {/* Visibility toggle */}
      <div className="flex items-center gap-3">
        <button
          type="button"
          role="switch"
          aria-checked={isPublic}
          onClick={() => setIsPublic((v) => !v)}
          className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 ${
            isPublic ? 'bg-primary-600' : 'bg-gray-200 dark:bg-gray-700'
          }`}
          aria-label="Toggle public visibility"
          data-testid="edit-public-toggle"
        >
          <span
            className={`inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform ${
              isPublic ? 'translate-x-6' : 'translate-x-1'
            }`}
          />
        </button>
        <span className="text-sm text-gray-700 dark:text-gray-300">
          {isPublic ? 'Public' : 'Private'}
        </span>
      </div>

      {saveError && (
        <p className="text-sm text-red-600 dark:text-red-400" role="alert" data-testid="save-error">
          {saveError}
        </p>
      )}

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-md border border-gray-300 px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
          aria-label="Cancel editing"
          data-testid="cancel-edit-button"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSaving}
          className="rounded-md bg-primary-600 px-5 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:cursor-not-allowed disabled:opacity-50"
          aria-label="Save recipe changes"
          data-testid="save-edit-button"
        >
          {isSaving ? 'Saving…' : 'Save changes'}
        </button>
      </div>
    </form>
  )
}
