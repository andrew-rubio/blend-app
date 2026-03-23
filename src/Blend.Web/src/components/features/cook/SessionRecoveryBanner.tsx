'use client'

import { Button } from '@/components/ui/Button'

export interface SessionRecoveryBannerProps {
  onResume: () => void
  onDismiss: () => void
}

export function SessionRecoveryBanner({ onResume, onDismiss }: SessionRecoveryBannerProps) {
  return (
    <div
      className="flex items-center justify-between rounded-md bg-amber-50 px-4 py-3 text-sm text-amber-800 dark:bg-amber-900/30 dark:text-amber-300"
      role="alert"
      data-testid="session-recovery-banner"
    >
      <p>You have a paused cooking session. Resume it?</p>
      <div className="flex gap-2">
        <Button size="sm" variant="primary" onClick={onResume} data-testid="session-recovery-resume">
          Resume
        </Button>
        <Button size="sm" variant="ghost" onClick={onDismiss} data-testid="session-recovery-dismiss">
          Dismiss
        </Button>
      </div>
    </div>
  )
}
