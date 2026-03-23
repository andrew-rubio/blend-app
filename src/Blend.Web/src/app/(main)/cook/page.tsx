'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useActiveSession, useCreateSession } from '@/hooks/useCookMode'
import { CookModeContainer } from '@/components/features/cook/CookModeContainer'
import { SessionRecoveryBanner } from '@/components/features/cook/SessionRecoveryBanner'

export default function CookPage() {
  const router = useRouter()
  const { data: activeSession, isLoading, error } = useActiveSession()
  const createSession = useCreateSession()
  const [sessionId, setSessionId] = useState<string | null>(null)
  const [dismissed, setDismissed] = useState(false)

  useEffect(() => {
    if (!isLoading && !error && activeSession) {
      if (activeSession.status === 'Active') {
        setSessionId(activeSession.id)
      }
    }
  }, [activeSession, isLoading, error])

  // No active session — create one
  useEffect(() => {
    if (!isLoading && error && (error as { status?: number })?.status === 404 && !sessionId) {
      createSession.mutate({}, {
        onSuccess: (session) => setSessionId(session.id),
        onError: () => router.push('/login'),
      })
    }
    // createSession.mutate and router.push are stable refs; intentionally omitted to avoid re-triggering
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoading, error, sessionId])

  if (isLoading || createSession.isPending) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <p className="text-gray-500 dark:text-gray-400">Starting cook mode…</p>
      </div>
    )
  }

  if (activeSession?.status === 'Paused' && !dismissed && !sessionId) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <SessionRecoveryBanner
          onResume={() => setSessionId(activeSession.id)}
          onDismiss={() => setDismissed(true)}
        />
      </div>
    )
  }

  if (!sessionId) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <p className="text-gray-500 dark:text-gray-400">Starting cook mode…</p>
      </div>
    )
  }

  return <CookModeContainer sessionId={sessionId} />
}
