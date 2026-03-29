'use client'

import { useParams } from 'next/navigation'
import { WrapUpWizard } from '@/components/features/cook/WrapUpWizard'

export function generateStaticParams() {
  return []
}

export default function WrapUpPage() {
  const params = useParams<{ id: string }>()
  const sessionId = params.id

  return <WrapUpWizard sessionId={sessionId} />
}
