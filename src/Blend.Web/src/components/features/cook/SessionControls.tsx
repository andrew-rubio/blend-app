'use client'

import { Button } from '@/components/ui/Button'

export interface SessionControlsProps {
  onPause: () => void
  onFinish: () => void
  isPausing: boolean
  isFinishing: boolean
}

export function SessionControls({ onPause, onFinish, isPausing, isFinishing }: SessionControlsProps) {
  return (
    <div className="flex items-center gap-3" data-testid="session-controls">
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
