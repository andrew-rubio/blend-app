import type { Metadata } from 'next'
import { ProfileContainer } from '@/components/features/profile/ProfileContainer'

export const metadata: Metadata = {
  title: 'My Profile',
}

export default function ProfilePage() {
  return <ProfileContainer />
}
