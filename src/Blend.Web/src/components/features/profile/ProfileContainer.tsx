'use client'

import { useState } from 'react'
import { ProfileHeader } from './ProfileHeader'
import { ProfileEditForm } from './ProfileEditForm'
import { ProfileRecipeTabs } from './ProfileRecipeTabs'
import { useMyProfile } from '@/hooks/useProfile'

export function ProfileContainer() {
  const [isEditing, setIsEditing] = useState(false)
  const { data: profile, isLoading, error } = useMyProfile()

  if (isLoading) {
    return (
      <div aria-label="Loading profile" className="mx-auto max-w-4xl px-4 py-8">
        <div className="animate-pulse space-y-4">
          <div className="h-32 rounded-xl bg-gray-100 dark:bg-gray-800" />
          <div className="h-10 rounded-lg bg-gray-100 dark:bg-gray-800" />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="aspect-[4/3] rounded-xl bg-gray-100 dark:bg-gray-800" />
            ))}
          </div>
        </div>
      </div>
    )
  }

  if (error || !profile) {
    const errStatus = error && typeof error === 'object' && 'status' in error
      ? (error as { status: number }).status
      : 0
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 text-center">
        <p className="text-gray-500 dark:text-gray-400">
          {errStatus === 401 ? 'Please sign in to view your profile.' : 'Failed to load profile.'}
        </p>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <ProfileHeader profile={profile} onEditClick={() => setIsEditing(true)} />
      <ProfileRecipeTabs />
      {isEditing && (
        <ProfileEditForm
          profile={profile}
          onCancel={() => setIsEditing(false)}
          onSaved={() => setIsEditing(false)}
        />
      )}
    </div>
  )
}
