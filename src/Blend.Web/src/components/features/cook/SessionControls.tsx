'use client'

import { Button } from '@/components/ui/Button'

export interface SessionControlsProps {
  onPause: () => void
  onFinish: () => void
  onFindRecipes?: () => void
  isPausing: boolean
  isFinishing: boolean
  hasIngredients?: boolean
}

export function SessionControls({ onPause, onFinish, onFindRecipes, isPausing, isFinishing, hasIngredients }: SessionControlsProps) {
  return (
    <div className="flex items-center gap-3" data-testid="session-controls">
      {onFindRecipes && (
        <Button
          variant="outline"
          onClick={onFindRecipes}
          disabled={isPausing || isFinishing || !hasIngredients}
          data-testid="session-controls-find-recipes"
        >
          Find recipes
        </Button>
      )}
      <Button
        variant="outline"
        onClick={onPause}
        isLoading={isPausing}
        disabled={isPausing || isFinishing}
        data-testid="session-controls-pause"
      >
        Pause session
      </Button>
      <Button
        variant="primary"
        onClick={onFinish}
        isLoading={isFinishing}
        disabled={isPausing || isFinishing}
        data-testid="session-controls-finish"
      >
        Finish cooking
      </Button>
    </div>
  )
}
