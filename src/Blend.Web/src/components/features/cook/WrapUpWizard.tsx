'use client'

import { useRouter } from 'next/navigation'
import { useWrapUpStore, WRAP_UP_STEPS, WRAP_UP_STEP_LABELS } from '@/stores/wrapUpStore'
import { useSession, useSubmitFeedback, usePublishSession } from '@/hooks/useCookMode'
import { SessionSummaryStep } from './wrap-up/SessionSummaryStep'
import { PairingFeedbackStep } from './wrap-up/PairingFeedbackStep'
import { PhotoUploadStep } from './wrap-up/PhotoUploadStep'
import { PublishStep } from './wrap-up/PublishStep'
import { CompletionStep } from './wrap-up/CompletionStep'

interface WrapUpWizardProps {
  sessionId: string
}

/**
 * Multi-step post-cook wrap-up wizard (COOK-30 through COOK-45).
 * Steps: Summary → Pairing Feedback → Photo Upload → Publish → Completion.
 */
export function WrapUpWizard({ sessionId }: WrapUpWizardProps) {
  const router = useRouter()
  const { data: session, isLoading, error } = useSession(sessionId)
  const submitFeedback = useSubmitFeedback(sessionId)
  const publishSession = usePublishSession(sessionId)

  const {
    currentStepIndex,
    feedbackItems,
    photos,
    primaryPhotoIndex,
    shouldPublish,
    publishForm,
    publishedRecipeId,
    nextStep,
    prevStep,
    setFeedbackRating,
    addPhoto,
    removePhoto,
    setPrimaryPhoto,
    setShouldPublish,
    setPublishField,
    addDirection,
    updateDirection,
    removeDirection,
    setPublishedRecipeId,
    reset,
  } = useWrapUpStore()

  if (isLoading) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8" data-testid="wrap-up-loading">
        <div className="animate-pulse">
          <div className="mb-4 h-8 w-48 rounded bg-gray-200 dark:bg-gray-700" />
          <div className="h-64 rounded bg-gray-200 dark:bg-gray-700" />
        </div>
      </div>
    )
  }

  if (error || !session) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8" data-testid="wrap-up-error">
        <p className="text-red-600 dark:text-red-400">
          {(error as { message?: string })?.message ?? 'Failed to load cooking session'}
        </p>
      </div>
    )
  }

  const currentStep = WRAP_UP_STEPS[currentStepIndex]
  const totalSteps = WRAP_UP_STEPS.length

  async function handleFeedbackNext() {
    if (feedbackItems.length > 0) {
      await submitFeedback.mutateAsync({ feedback: feedbackItems })
    }
    nextStep()
  }

  async function handlePublishNext(skipPublish: boolean) {
    if (skipPublish || !shouldPublish) {
      nextStep()
      return
    }

    const result = await publishSession.mutateAsync({
      title: publishForm.title,
      description: publishForm.description || undefined,
      directions: publishForm.directions,
      photos,
      cuisineType: publishForm.cuisineType || undefined,
      tags: publishForm.tags,
      servings: publishForm.servings,
      prepTime: publishForm.prepTime,
      cookTime: publishForm.cookTime,
    })

    setPublishedRecipeId(result.recipeId)
    nextStep()
  }

  function handleReturnHome() {
    reset()
    router.push('/')
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-6" data-testid="wrap-up-wizard">
      {/* Header */}
      <div className="mb-6">
        <h1 className="mb-1 text-2xl font-bold text-gray-900 dark:text-white">Wrap Up</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          Step {currentStepIndex + 1} of {totalSteps} — {WRAP_UP_STEP_LABELS[currentStep]}
        </p>
      </div>

      {/* Progress bar */}
      <div
        className="mb-8 h-1.5 w-full overflow-hidden rounded-full bg-gray-200 dark:bg-gray-700"
        role="progressbar"
        aria-valuenow={currentStepIndex + 1}
        aria-valuemin={1}
        aria-valuemax={totalSteps}
        aria-label={`Step ${currentStepIndex + 1} of ${totalSteps}`}
      >
        <div
          className="h-full bg-primary-600 transition-all"
          style={{ width: `${((currentStepIndex + 1) / totalSteps) * 100}%` }}
        />
      </div>

      {/* Back button (not shown on first or last step) */}
      {currentStepIndex > 0 && currentStep !== 'completion' && (
        <div className="mb-4">
          <button
            type="button"
            onClick={prevStep}
            className="text-sm text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
            aria-label="Go to previous step"
            data-testid="back-button"
          >
            ← Back
          </button>
        </div>
      )}

      {/* Step content */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-700 dark:bg-gray-800">
        {currentStep === 'summary' && (
          <SessionSummaryStep session={session} onNext={nextStep} />
        )}

        {currentStep === 'feedback' && (
          <PairingFeedbackStep
            session={session}
            feedbackItems={feedbackItems}
            onRate={setFeedbackRating}
            onNext={() => void handleFeedbackNext()}
            onSkip={nextStep}
          />
        )}

        {currentStep === 'photos' && (
          <PhotoUploadStep
            photos={photos}
            primaryPhotoIndex={primaryPhotoIndex}
            onAdd={addPhoto}
            onRemove={removePhoto}
            onSetPrimary={setPrimaryPhoto}
            onNext={nextStep}
            onSkip={nextStep}
          />
        )}

        {currentStep === 'publish' && (
          <PublishStep
            session={session}
            shouldPublish={shouldPublish}
            form={publishForm}
            onTogglePublish={setShouldPublish}
            onFieldChange={(field, value) =>
              setPublishField(field as keyof typeof publishForm, value as never)
            }
            onAddDirection={addDirection}
            onUpdateDirection={updateDirection}
            onRemoveDirection={removeDirection}
            onNext={(skip) => void handlePublishNext(skip)}
            isPublishing={publishSession.isPending}
            publishError={
              publishSession.error
                ? (publishSession.error as { message?: string })?.message ?? 'Failed to publish'
                : null
            }
          />
        )}

        {currentStep === 'completion' && (
          <CompletionStep
            publishedRecipeId={publishedRecipeId}
            onReturnHome={handleReturnHome}
          />
        )}
      </div>
    </div>
  )
}
