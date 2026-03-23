'use client'

import type { CookingSession, RecipeDirectionRequest } from '@/types'

interface PublishStepProps {
  session: CookingSession
  shouldPublish: boolean
  form: {
    title: string
    description: string
    directions: RecipeDirectionRequest[]
    cuisineType: string
    tags: string[]
    servings: number
    prepTime: number
    cookTime: number
  }
  onTogglePublish: (value: boolean) => void
  onFieldChange: (field: string, value: unknown) => void
  onAddDirection: () => void
  onUpdateDirection: (index: number, text: string) => void
  onRemoveDirection: (index: number) => void
  onNext: (skipPublish: boolean) => void
  isPublishing?: boolean
  publishError?: string | null
}

/**
 * Step 4: Publish as Recipe (COOK-40 through COOK-44).
 * Toggle to publish as community recipe, with form for title, description,
 * directions, cuisine type, tags and timings.
 */
export function PublishStep({
  session,
  shouldPublish,
  form,
  onTogglePublish,
  onFieldChange,
  onAddDirection,
  onUpdateDirection,
  onRemoveDirection,
  onNext,
  isPublishing = false,
  publishError,
}: PublishStepProps) {
  const allIngredients = [
    ...session.dishes.flatMap((d) => d.ingredients),
    ...session.addedIngredients,
  ]
  // Deduplicate by ingredientId
  const uniqueIngredients = allIngredients.filter(
    (ing, idx, arr) => arr.findIndex((x) => x.ingredientId === ing.ingredientId) === idx,
  )

  const canPublish =
    form.title.trim().length > 0 && form.directions.length > 0

  return (
    <div data-testid="publish-step">
      <h2 className="mb-1 text-xl font-semibold text-gray-900 dark:text-white">Publish Recipe</h2>
      <p className="mb-6 text-sm text-gray-500 dark:text-gray-400">
        Share your creation with the Blend community.
      </p>

      {/* Toggle */}
      <div className="mb-6 flex items-center gap-3">
        <button
          type="button"
          role="switch"
          aria-checked={shouldPublish}
          onClick={() => onTogglePublish(!shouldPublish)}
          className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 ${
            shouldPublish ? 'bg-primary-600' : 'bg-gray-200 dark:bg-gray-700'
          }`}
          aria-label="Toggle publish as community recipe"
          data-testid="publish-toggle"
        >
          <span
            className={`inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform ${
              shouldPublish ? 'translate-x-6' : 'translate-x-1'
            }`}
          />
        </button>
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
          Publish as community recipe
        </span>
      </div>

      {shouldPublish && (
        <div className="space-y-5" data-testid="publish-form">
          {/* Title */}
          <div>
            <label
              htmlFor="publish-title"
              className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
            >
              Title <span className="text-red-500" aria-hidden="true">*</span>
            </label>
            <input
              id="publish-title"
              type="text"
              value={form.title}
              onChange={(e) => onFieldChange('title', e.target.value)}
              placeholder="Give your recipe a name"
              maxLength={200}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
              aria-required="true"
              data-testid="publish-title-input"
            />
          </div>

          {/* Description */}
          <div>
            <label
              htmlFor="publish-description"
              className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
            >
              Description
            </label>
            <textarea
              id="publish-description"
              value={form.description}
              onChange={(e) => onFieldChange('description', e.target.value)}
              placeholder="What makes this dish special?"
              rows={3}
              className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
              data-testid="publish-description-input"
            />
          </div>

          {/* Ingredients (read-only, pre-populated from session) */}
          <div>
            <p className="mb-2 text-sm font-medium text-gray-700 dark:text-gray-300">
              Ingredients (from your session)
            </p>
            {uniqueIngredients.length > 0 ? (
              <ul className="rounded-md border border-gray-200 bg-gray-50 p-3 dark:border-gray-700 dark:bg-gray-900">
                {uniqueIngredients.map((ing) => (
                  <li key={ing.ingredientId} className="text-sm text-gray-600 dark:text-gray-300">
                    • {ing.name}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-gray-400 dark:text-gray-500">No ingredients in session.</p>
            )}
          </div>

          {/* Directions */}
          <div>
            <div className="mb-2 flex items-center justify-between">
              <p className="text-sm font-medium text-gray-700 dark:text-gray-300">
                Directions <span className="text-red-500" aria-hidden="true">*</span>
              </p>
              <button
                type="button"
                onClick={onAddDirection}
                className="text-sm text-primary-600 hover:underline dark:text-primary-400"
                aria-label="Add a direction step"
                data-testid="add-direction-button"
              >
                + Add step
              </button>
            </div>
            {form.directions.length === 0 ? (
              <p className="text-sm text-gray-400 dark:text-gray-500">
                Add at least one direction step.
              </p>
            ) : (
              <ol className="space-y-3" data-testid="directions-list">
                {form.directions.map((dir, index) => (
                  <li key={index} className="flex items-start gap-2">
                    <span className="mt-2 min-w-[1.5rem] text-sm font-medium text-gray-500 dark:text-gray-400">
                      {dir.stepNumber}.
                    </span>
                    <input
                      type="text"
                      value={dir.text}
                      onChange={(e) => onUpdateDirection(index, e.target.value)}
                      placeholder={`Step ${dir.stepNumber}`}
                      className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                      aria-label={`Direction step ${dir.stepNumber}`}
                      data-testid={`direction-input-${index}`}
                    />
                    <button
                      type="button"
                      onClick={() => onRemoveDirection(index)}
                      className="mt-2 text-sm text-red-500 hover:text-red-700"
                      aria-label={`Remove step ${dir.stepNumber}`}
                      data-testid={`remove-direction-${index}`}
                    >
                      ✕
                    </button>
                  </li>
                ))}
              </ol>
            )}
          </div>

          {/* Optional fields: cuisine, tags, servings, times */}
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
            <div>
              <label
                htmlFor="publish-cuisine"
                className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
              >
                Cuisine type
              </label>
              <input
                id="publish-cuisine"
                type="text"
                value={form.cuisineType}
                onChange={(e) => onFieldChange('cuisineType', e.target.value)}
                placeholder="e.g. Italian"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                data-testid="publish-cuisine-input"
              />
            </div>
            <div>
              <label
                htmlFor="publish-servings"
                className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
              >
                Servings
              </label>
              <input
                id="publish-servings"
                type="number"
                min={0}
                value={form.servings || ''}
                onChange={(e) => onFieldChange('servings', Number(e.target.value))}
                placeholder="2"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                data-testid="publish-servings-input"
              />
            </div>
            <div>
              <label
                htmlFor="publish-prep-time"
                className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
              >
                Prep time (min)
              </label>
              <input
                id="publish-prep-time"
                type="number"
                min={0}
                value={form.prepTime || ''}
                onChange={(e) => onFieldChange('prepTime', Number(e.target.value))}
                placeholder="15"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                data-testid="publish-prep-time-input"
              />
            </div>
            <div>
              <label
                htmlFor="publish-cook-time"
                className="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-300"
              >
                Cook time (min)
              </label>
              <input
                id="publish-cook-time"
                type="number"
                min={0}
                value={form.cookTime || ''}
                onChange={(e) => onFieldChange('cookTime', Number(e.target.value))}
                placeholder="30"
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-primary-500 focus:outline-none focus:ring-1 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
                data-testid="publish-cook-time-input"
              />
            </div>
          </div>

          {publishError && (
            <p className="text-sm text-red-600 dark:text-red-400" role="alert" data-testid="publish-error">
              {publishError}
            </p>
          )}
        </div>
      )}

      <div className="mt-6 flex justify-end gap-3">
        <button
          type="button"
          onClick={() => onNext(true)}
          className="rounded-md border border-gray-300 px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
          aria-label="Skip publishing"
          data-testid="publish-skip-button"
        >
          Skip
        </button>
        <button
          type="button"
          onClick={() => onNext(false)}
          disabled={shouldPublish && (!canPublish || isPublishing)}
          className="rounded-md bg-primary-600 px-5 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:cursor-not-allowed disabled:opacity-50"
          aria-label={shouldPublish ? 'Publish recipe and finish' : 'Finish without publishing'}
          data-testid="publish-next-button"
        >
          {isPublishing ? 'Publishing…' : shouldPublish ? 'Publish' : 'Finish'}
        </button>
      </div>
    </div>
  )
}
