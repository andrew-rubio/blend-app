'use client'

import { useParams } from 'next/navigation'
import { PublicProfileContainer } from '@/components/features/profile/PublicProfileContainer'

export function generateStaticParams() {
  return []
}

export default function UserProfilePage() {
  const { userId } = useParams<{ userId: string }>()
  return <PublicProfileContainer userId={userId} />
}
