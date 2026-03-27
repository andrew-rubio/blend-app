import type { Metadata } from 'next'
import { notFound } from 'next/navigation'
import { PublicProfileContainer } from '@/components/features/profile/PublicProfileContainer'
import { fetchProfileForMetadata, buildProfileMetadata } from '@/lib/metadata'

interface PageProps {
  params: Promise<{ userId: string }>
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { userId } = await params
  try {
    const profile = await fetchProfileForMetadata(userId)
    return buildProfileMetadata(profile, userId)
  } catch (err) {
    const status = (err as { status?: number }).status
    if (status === 404) {
      notFound()
    }
    return { title: 'Profile' }
  }
}

export default async function UserProfilePage({ params }: PageProps) {
  const { userId } = await params
  return <PublicProfileContainer userId={userId} />
}
