'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useCookModeStore } from '@/stores/cookModeStore'
import {
  useSession,
  useAddIngredient,
  useRemoveIngredient,
  useAddDish,
  useRemoveDish,
  usePauseSession,
  useCompleteSession,
} from '@/hooks/useCookMode'
import { DishTabs } from './DishTabs'
import { IngredientWorkspace } from './IngredientWorkspace'
import { IngredientSearch } from './IngredientSearch'
import { SuggestionsPanel } from './SuggestionsPanel'
import { IngredientDetailModal } from './IngredientDetailModal'
import { DishNotes } from './DishNotes'
import { SessionControls } from './SessionControls'
import type { IngredientSearchResult, SmartSuggestion } from '@/types'

export interface CookModeContainerProps {
  sessionId: string
}

export function CookModeContainer({ sessionId }: CookModeContainerProps) {
  const router = useRouter()
  const { data: session, isLoading, error } = useSession(sessionId)
  const {
    activeDishId,
    selectedIngredientId,
    isDetailModalOpen,
    isSuggestionsPanelOpen,
    setActiveDishId,
    openIngredientDetail,
    closeIngredientDetail,
    openSuggestionsPanel,
    closeSuggestionsPanel,
  } = useCookModeStore()

  const addIngredient = useAddIngredient(sessionId)
  const removeIngredient = useRemoveIngredient(sessionId)
  const addDish = useAddDish(sessionId)
  const removeDish = useRemoveDish(sessionId)
  const pauseSession = usePauseSession(sessionId)
  const completeSession = useCompleteSession(sessionId)

  useEffect(() => {
    if (session && session.dishes.length > 0 && !activeDishId) {
      setActiveDishId(session.dishes[0].dishId)
    }
  }, [session, activeDishId, setActiveDishId])

  if (isLoading) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8" data-testid="cook-mode-loading">
        <div className="animate-pulse">
          <div className="mb-4 h-8 w-48 rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-64 rounded bg-gray-200 dark:bg-gray-700" />
        </div>
      </div>
    )
  }

  if (error || !session) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8" data-testid="cook-mode-error">
        <p className="text-red-600 dark:text-red-400">
          {(error as { message?: string })?.message ?? 'Failed to load cooking session'}
        </p>
      </div>
    )
  }

  const activeDish = session.dishes.find((d) => d.dishId === activeDishId) ?? session.dishes[0]
  const activeIngredients = activeDish ? activeDish.ingredients : session.addedIngredients

  function handleAddIngredient(ingredient: IngredientSearchResult) {
    addIngredient.mutate({
      ingredientId: ingredient.id,
      name: ingredient.name,
      dishId: activeDishId ?? undefined,
    })
  }

  function handleRemoveIngredient(ingredientId: string, dishId?: string) {
    removeIngredient.mutate({ ingredientId, dishId })
  }

  function handleAddSuggestion(suggestion: SmartSuggestion) {
    addIngredient.mutate({
      ingredientId: suggestion.ingredientId,
      name: suggestion.name,
      dishId: activeDishId ?? undefined,
    })
  }

  function handleAddDish() {
    addDish.mutate({ name: 'New Dish' })
  }

  function handleRemoveDish(dishId: string) {
    removeDish.mutate(dishId)
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  function handleRenameDish(_dishId: string, _name: string) {
    // TODO: no API yet
  }

  function handlePause() {
    pauseSession.mutate(undefined, {
      onSuccess: () => router.push('/'),
    })
  }

  function handleFinish() {
    completeSession.mutate(undefined, {
      onSuccess: () => router.push('/'),
    })
  }

  function handleSaveDishNotes(notes: string) {
    // TODO: no API for notes yet; placeholder
    void notes
  }

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-6" data-testid="cook-mode-container">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Cook Mode</h1>
        <SessionControls
          onPause={handlePause}
          onFinish={handleFinish}
          isPausing={pauseSession.isPending}
          isFinishing={completeSession.isPending}
        />
      </div>

      {/* Main content */}
      <div className="flex gap-6">
        <div className="flex flex-1 flex-col gap-4">
          {/* Dish tabs */}
          {session.dishes.length > 0 && (
            <DishTabs
              dishes={session.dishes}
              activeDishId={activeDishId}
              onSelect={setActiveDishId}
              onAdd={handleAddDish}
              onRemove={handleRemoveDish}
              onRename={handleRenameDish}
            />
          )}

          {/* Ingredient workspace */}
          <IngredientWorkspace
            ingredients={activeIngredients}
            onRemove={handleRemoveIngredient}
            onDetail={openIngredientDetail}
            dishId={activeDishId ?? undefined}
          />

          {/* Ingredient search */}
          <IngredientSearch
            onAdd={handleAddIngredient}
            disabled={addIngredient.isPending}
          />

          {/* Dish notes */}
          {activeDish && (
            <DishNotes
              notes={activeDish.notes ?? ''}
              onSave={handleSaveDishNotes}
            />
          )}
        </div>

        {/* Suggestions sidebar */}
        <div className="hidden w-72 flex-shrink-0 lg:block">
          <div className="sticky top-6 rounded-lg border border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800">
            <div className="flex items-center justify-between border-b border-gray-200 px-4 py-3 dark:border-gray-700">
              <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Suggestions</h2>
            </div>
            <SuggestionsPanel
              sessionId={sessionId}
              dishId={activeDishId ?? undefined}
              onAdd={handleAddSuggestion}
            />
          </div>
        </div>

        {/* Mobile suggestions toggle */}
        <div className="lg:hidden">
          <button
            type="button"
            onClick={isSuggestionsPanelOpen ? closeSuggestionsPanel : openSuggestionsPanel}
            className="text-sm text-primary-600 dark:text-primary-400"
            data-testid="suggestions-toggle"
          >
            {isSuggestionsPanelOpen ? 'Hide suggestions' : 'Show suggestions'}
          </button>
          {isSuggestionsPanelOpen && (
            <SuggestionsPanel
              sessionId={sessionId}
              dishId={activeDishId ?? undefined}
              onAdd={handleAddSuggestion}
            />
          )}
        </div>
      </div>

      {/* Ingredient detail modal */}
      {isDetailModalOpen && selectedIngredientId && (
        <IngredientDetailModal
          sessionId={sessionId}
          ingredientId={selectedIngredientId}
          onClose={closeIngredientDetail}
        />
      )}
    </div>
  )
}
