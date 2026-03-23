import type { Metadata } from 'next'
import { PublicProfileContainer } from '@/components/features/profile/PublicProfileContainer'

export const metadata: Metadata = {
  title: 'Profile',
}

interface PageProps {
  params: Promise<{ userId: string }>
}

export default async function UserProfilePage({ params }: PageProps) {
  const { userId } = await params
  return <PublicProfileContainer userId={userId} />
}
