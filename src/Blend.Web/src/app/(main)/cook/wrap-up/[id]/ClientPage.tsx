'use client'

import { useParams } from 'next/navigation'
import { WrapUpWizard } from '@/components/features/cook/WrapUpWizard'

export default function WrapUpClientPage() {
  const params = useParams<{ id: string }>()
  const sessionId = params.id

  return <WrapUpWizard sessionId={sessionId} />
}
