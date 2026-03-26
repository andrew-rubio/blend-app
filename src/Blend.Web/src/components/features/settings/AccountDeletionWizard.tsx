'use client'

import { useState } from 'react'
import { clsx } from 'clsx'
import { useRequestAccountDeletion } from '@/hooks/useSettings'
import { useAuthStore } from '@/stores/authStore'

type DeletionStep = 'warning' | 'reauth' | 'confirm' | 'success'

const CONFIRM_KEYWORD = 'DELETE'

export interface AccountDeletionWizardProps {
  onClose: () => void
}

/**
 * Multi-step account deletion flow (SETT-17 through SETT-24).
 * Steps: Warning → Re-authentication → Confirmation → Success.
 */
export function AccountDeletionWizard({ onClose }: AccountDeletionWizardProps) {
  const [step, setStep] = useState<DeletionStep>('warning')
  const [password, setPassword] = useState('')
  const [confirmText, setConfirmText] = useState('')
  const [passwordError, setPasswordError] = useState<string | null>(null)

  const { mutate: requestDeletion, isPending, error } = useRequestAccountDeletion()
  const logout = useAuthStore((s) => s.logout)

  const apiErrorMessage =
    error && typeof error === 'object' && 'message' in error
      ? (error as { message: string }).message
      : null

  function handleWarningNext() {
    setStep('reauth')
  }

  function handleReauthNext() {
    if (!password.trim()) {
      setPasswordError('Please enter your password.')
      return
    }
    setPasswordError(null)
    setStep('confirm')
  }

  function handleConfirm() {
    if (confirmText !== CONFIRM_KEYWORD) return
    requestDeletion(
      { password },
      {
        onSuccess: () => {
          setStep('success')
        },
      }
    )
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Delete account"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-sm"
    >
      <div className="w-full max-w-md rounded-2xl bg-white shadow-2xl dark:bg-gray-900">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
            {step === 'success' ? 'Deletion scheduled' : 'Delete account'}
          </h2>
          {step !== 'success' && (
            <button
              onClick={onClose}
              aria-label="Close"
              className="rounded-md p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>

        <div className="px-6 py-5">
          {/* Step 1: Warning */}
          {step === 'warning' && (
            <div>
              <div className="mb-4 flex items-center justify-center">
                <div className="flex h-16 w-16 items-center justify-center rounded-full bg-red-100 dark:bg-red-900/30">
                  <svg className="h-8 w-8 text-red-600 dark:text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
                  </svg>
                </div>
              </div>
              <h3 className="mb-2 text-center text-base font-semibold text-gray-900 dark:text-white">
                This action is permanent
              </h3>
              <p className="mb-3 text-sm text-gray-600 dark:text-gray-400">
                Deleting your account will permanently remove:
              </p>
              <ul className="mb-4 list-inside list-disc space-y-1 text-sm text-gray-600 dark:text-gray-400">
                <li>Your profile and all personal data</li>
                <li>All recipes you&apos;ve created</li>
                <li>Your preferences, friends, and activity</li>
              </ul>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                You have a <strong>30-day grace period</strong> after requesting deletion to cancel.
                After that, recovery is not possible.
              </p>
            </div>
          )}

          {/* Step 2: Re-authentication */}
          {step === 'reauth' && (
            <div>
              <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
                Please enter your password to confirm your identity before proceeding.
              </p>
              <label htmlFor="deletion-password" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Password
              </label>
              <input
                id="deletion-password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="current-password"
                aria-describedby={passwordError ? 'deletion-password-error' : undefined}
                className={clsx(
                  'mt-1 block w-full rounded-lg border px-3 py-2 text-sm',
                  'focus:outline-none focus:ring-2 focus:ring-primary-500',
                  'dark:bg-gray-800 dark:text-white',
                  passwordError ? 'border-red-400 dark:border-red-500' : 'border-gray-300 dark:border-gray-600'
                )}
              />
              {passwordError && (
                <p id="deletion-password-error" role="alert" className="mt-1 text-xs text-red-500">
                  {passwordError}
                </p>
              )}
            </div>
          )}

          {/* Step 3: Confirmation */}
          {step === 'confirm' && (
            <div>
              <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
                To confirm, type <strong className="text-gray-900 dark:text-white">{CONFIRM_KEYWORD}</strong> in the box below.
              </p>
              <label htmlFor="deletion-confirm" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Confirmation
              </label>
              <input
                id="deletion-confirm"
                type="text"
                value={confirmText}
                onChange={(e) => setConfirmText(e.target.value)}
                placeholder={CONFIRM_KEYWORD}
                aria-label={`Type ${CONFIRM_KEYWORD} to confirm deletion`}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-500 dark:border-gray-600 dark:bg-gray-800 dark:text-white"
              />
              {apiErrorMessage && (
                <div role="alert" className="mt-3 rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300">
                  {apiErrorMessage}
                </div>
              )}
            </div>
          )}

          {/* Step 4: Success */}
          {step === 'success' && (
            <div className="text-center">
              <div className="mb-4 flex items-center justify-center">
                <div className="flex h-16 w-16 items-center justify-center rounded-full bg-orange-100 dark:bg-orange-900/30">
                  <svg className="h-8 w-8 text-orange-600 dark:text-orange-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </div>
              <p className="mb-2 text-sm text-gray-600 dark:text-gray-400">
                Your account will be <strong>permanently deleted in 30 days</strong>.
              </p>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                You can cancel this by logging back in during the grace period and selecting &quot;Cancel deletion&quot;.
              </p>
            </div>
          )}
        </div>

        {/* Footer actions */}
        <div className="flex justify-end gap-3 border-t border-gray-200 px-6 py-4 dark:border-gray-700">
          {step === 'warning' && (
            <>
              <button
                onClick={onClose}
                className={clsx(
                  'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700',
                  'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
                  'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
                )}
              >
                Cancel
              </button>
              <button
                onClick={handleWarningNext}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
              >
                Continue
              </button>
            </>
          )}

          {step === 'reauth' && (
            <>
              <button
                onClick={() => setStep('warning')}
                className={clsx(
                  'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700',
                  'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
                  'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
                )}
              >
                Back
              </button>
              <button
                onClick={handleReauthNext}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2"
              >
                Next
              </button>
            </>
          )}

          {step === 'confirm' && (
            <>
              <button
                onClick={() => setStep('reauth')}
                disabled={isPending}
                className={clsx(
                  'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700',
                  'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
                  'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50'
                )}
              >
                Back
              </button>
              <button
                onClick={handleConfirm}
                disabled={isPending || confirmText !== CONFIRM_KEYWORD}
                className={clsx(
                  'rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white',
                  'hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2',
                  'disabled:opacity-50',
                  (isPending || confirmText !== CONFIRM_KEYWORD) && 'cursor-not-allowed'
                )}
              >
                {isPending ? 'Deleting…' : 'Delete my account'}
              </button>
            </>
          )}

          {step === 'success' && (
            <button
              onClick={() => {
                logout()
                onClose()
              }}
              className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 dark:bg-gray-700 dark:hover:bg-gray-600"
            >
              Sign out
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
