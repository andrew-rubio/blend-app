import { clsx } from 'clsx'
import { WIZARD_STEPS, WIZARD_STEP_LABELS, type WizardStep } from '@/stores/preferenceStore'

export interface WizardProgressProps {
  currentStepIndex: number
}

/** Progress indicator showing current step label and step dots. */
export function WizardProgress({ currentStepIndex }: WizardProgressProps) {
  return (
    <nav aria-label="Preference wizard progress">
      <p className="mb-3 text-center text-sm text-gray-500 dark:text-gray-400">
        Step {currentStepIndex + 1} of {WIZARD_STEPS.length}
      </p>
      <ol className="flex items-center justify-center gap-2" role="list">
        {WIZARD_STEPS.map((step: WizardStep, idx: number) => {
          const isCompleted = idx < currentStepIndex
          const isCurrent = idx === currentStepIndex
          return (
            <li key={step} className="flex flex-col items-center gap-1">
              <span
                aria-current={isCurrent ? 'step' : undefined}
                aria-label={`${WIZARD_STEP_LABELS[step]}${isCompleted ? ' (completed)' : isCurrent ? ' (current)' : ''}`}
                className={clsx(
                  'flex h-3 w-3 rounded-full transition-colors',
                  isCompleted && 'bg-primary-600',
                  isCurrent && 'bg-primary-600 ring-2 ring-primary-300 ring-offset-1',
                  !isCompleted && !isCurrent && 'bg-gray-300 dark:bg-gray-600'
                )}
              />
            </li>
          )
        })}
      </ol>
      <p className="mt-2 text-center text-base font-semibold text-gray-900 dark:text-white">
        {WIZARD_STEP_LABELS[WIZARD_STEPS[currentStepIndex]]}
      </p>
    </nav>
  )
}
