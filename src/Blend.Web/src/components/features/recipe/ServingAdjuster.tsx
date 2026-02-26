'use client'

import { Button } from '@/components/ui/Button'

interface ServingAdjusterProps {
  servings: number
  onServingsChange: (servings: number) => void
}

export function ServingAdjuster({ servings, onServingsChange }: ServingAdjusterProps) {
  const decrement = () => {
    if (servings > 1) onServingsChange(servings - 1)
  }
  const increment = () => {
    onServingsChange(servings + 1)
  }

  return (
    <div className="flex items-center gap-3">
      <span className="text-sm font-medium text-gray-700">Servings:</span>
      <div className="flex items-center gap-2">
        <Button
          variant="secondary"
          size="sm"
          onClick={decrement}
          disabled={servings <= 1}
          aria-label="Decrease servings"
        >
          −
        </Button>
        <span className="w-8 text-center text-lg font-semibold" aria-live="polite">
          {servings}
        </span>
        <Button
          variant="secondary"
          size="sm"
          onClick={increment}
          aria-label="Increase servings"
        >
          +
        </Button>
      </div>
    </div>
  )
}
