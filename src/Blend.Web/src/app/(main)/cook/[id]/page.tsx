'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { useCreateSession, useActiveSession } from '@/hooks/useCookMode'
import { CookModeContainer } from '@/components/features/cook/CookModeContainer'

export default function CookRecipePage() {
  const params = useParams<{ id: string }>()
  const recipeId = params.id
  const router = useRouter()
  const createSession = useCreateSession()
  const { data: activeSession, isLoading: activeLoading } = useActiveSession()
  const [sessionId, setSessionId] = useState<string | null>(null)

  useEffect(() => {
    if (activeLoading) return
    createSession.mutate(
      { recipeId },
      {
        onSuccess: (session) => setSessionId(session.id),
        onError: (err) => {
          // 409 = existing session
          if ((err as { status?: number })?.status === 409 && activeSession) {
            setSessionId(activeSession.id)
          } else {
            router.push('/login')
          }
        },
      }
    )
    // createSession.mutate, router.push, and activeSession are stable or captured via closure;
    // intentionally omitted to avoid infinite re-triggering
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [recipeId, activeLoading])

  if (createSession.isPending || !sessionId) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8">
        <p className="text-gray-500 dark:text-gray-400">Starting cook mode…</p>
      </div>
    )
  }

  return <CookModeContainer sessionId={sessionId} />
}
