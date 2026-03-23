'use client'

import Image from 'next/image'
import { clsx } from 'clsx'
import type { MyProfile } from '@/types'

export interface ProfileHeaderProps {
  profile: MyProfile
  onEditClick: () => void
}

export function ProfileHeader({ profile, onEditClick }: ProfileHeaderProps) {
  const joinYear = new Date(profile.joinDate).getFullYear()

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-700 dark:bg-gray-900">
      {/* Avatar + name row */}
      <div className="flex flex-col items-center gap-4 sm:flex-row sm:items-start">
        <div className="relative h-20 w-20 flex-shrink-0 overflow-hidden rounded-full bg-gray-100 dark:bg-gray-800">
          {profile.avatarUrl ? (
            <Image
              src={profile.avatarUrl}
              alt={`${profile.displayName} avatar`}
              fill
              sizes="80px"
              className="object-cover"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center" aria-hidden="true">
              <svg className="h-10 w-10 text-gray-400" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z" />
              </svg>
            </div>
          )}
        </div>

        <div className="flex-1 text-center sm:text-left">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{profile.displayName}</h1>
          {profile.bio && (
            <p className="mt-1 text-sm text-gray-600 dark:text-gray-400">{profile.bio}</p>
          )}
          <p className="mt-1 text-xs text-gray-400 dark:text-gray-500">Member since {joinYear}</p>
        </div>

        <button
          onClick={onEditClick}
          aria-label="Edit profile"
          className={clsx(
            'rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium',
            'text-gray-700 transition-colors hover:bg-gray-50',
            'dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700',
            'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2'
          )}
        >
          Edit profile
        </button>
      </div>

      {/* Stats bar */}
      <div
        aria-label="Profile stats"
        className="mt-6 grid grid-cols-2 gap-3 border-t border-gray-200 pt-4 dark:border-gray-700 sm:grid-cols-4"
      >
        {[
          { label: 'Recipes', value: profile.recipeCount },
          { label: 'Likes received', value: profile.likeCount },
          { label: 'Followers', value: profile.followerCount },
          { label: 'Following', value: profile.followingCount },
        ].map(({ label, value }) => (
          <div key={label} className="text-center">
            <p className="text-xl font-bold text-gray-900 dark:text-white">{value.toLocaleString()}</p>
            <p className="text-xs text-gray-500 dark:text-gray-400">{label}</p>
          </div>
        ))}
      </div>
    </div>
  )
}
