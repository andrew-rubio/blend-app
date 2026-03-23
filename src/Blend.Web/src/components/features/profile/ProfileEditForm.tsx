'use client'

import { useState, useRef } from 'react'
import Image from 'next/image'
import { clsx } from 'clsx'
import type { MyProfile } from '@/types'
import { useUpdateProfile, useAvatarUpload } from '@/hooks/useProfile'

export interface ProfileEditFormProps {
  profile: MyProfile
  onCancel: () => void
  onSaved: () => void
}

const DISPLAY_NAME_MIN = 2
const DISPLAY_NAME_MAX = 50
const BIO_MAX = 500

export function ProfileEditForm({ profile, onCancel, onSaved }: ProfileEditFormProps) {
  const [displayName, setDisplayName] = useState(profile.displayName)
  const [bio, setBio] = useState(profile.bio ?? '')
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null)
  const [avatarFile, setAvatarFile] = useState<File | null>(null)
  const [fieldErrors, setFieldErrors] = useState<{ displayName?: string; bio?: string }>({})
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { mutate: updateProfile, isPending: isSaving, error: saveError } = useUpdateProfile()
  const { mutateAsync: uploadAvatar, isPending: isUploading } = useAvatarUpload()

  const isPending = isSaving || isUploading

  function validate(): boolean {
    const errors: { displayName?: string; bio?: string } = {}
    if (displayName.length < DISPLAY_NAME_MIN || displayName.length > DISPLAY_NAME_MAX) {
      errors.displayName = `Display name must be between ${DISPLAY_NAME_MIN} and ${DISPLAY_NAME_MAX} characters.`
    }
    if (bio.length > BIO_MAX) {
      errors.bio = `Bio must be at most ${BIO_MAX} characters.`
    }
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  function handleAvatarChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    setAvatarFile(file)
    const url = URL.createObjectURL(file)
    setAvatarPreview(url)
  }

  async function handleSave() {
    if (!validate()) return

    let avatarUrl = profile.avatarUrl
    if (avatarFile) {
      try {
        avatarUrl = await uploadAvatar(avatarFile)
      } catch {
        // Upload error will surface via useAvatarUpload error state
        return
      }
    }

    updateProfile(
      { displayName: displayName.trim(), bio: bio.trim() || undefined, avatarUrl },
      { onSuccess: onSaved }
    )
  }

  const errorMessage =
    saveError && typeof saveError === 'object' && 'message' in saveError
      ? (saveError as { message: string }).message
      : null

  const avatarSrc = avatarPreview ?? profile.avatarUrl

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Edit profile"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    >
      <div className="w-full max-w-md rounded-xl bg-white shadow-xl dark:bg-gray-900">
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Edit profile</h2>
          <button
            onClick={onCancel}
            aria-label="Close edit form"
            className="rounded-md p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 focus:outline-none focus:ring-2 focus:ring-primary-500"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2} aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div className="px-6 py-4 space-y-4">
          {/* Avatar upload */}
          <div className="flex flex-col items-center gap-3">
            <div className="relative h-20 w-20 overflow-hidden rounded-full bg-gray-100 dark:bg-gray-800">
              {avatarSrc ? (
                <Image src={avatarSrc} alt="Avatar preview" fill sizes="80px" className="object-cover" />
              ) : (
                <div className="flex h-full w-full items-center justify-center" aria-hidden="true">
                  <svg className="h-10 w-10 text-gray-400" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M12 12c2.7 0 4.8-2.1 4.8-4.8S14.7 2.4 12 2.4 7.2 4.5 7.2 7.2 9.3 12 12 12zm0 2.4c-3.2 0-9.6 1.6-9.6 4.8v2.4h19.2v-2.4c0-3.2-6.4-4.8-9.6-4.8z" />
                  </svg>
                </div>
              )}
            </div>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              aria-label="Upload avatar"
              className="sr-only"
              onChange={handleAvatarChange}
            />
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={isPending}
              className={clsx(
                'rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700',
                'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
                'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
                'disabled:opacity-50'
              )}
            >
              {isUploading ? 'Uploading…' : 'Change photo'}
            </button>
          </div>

          {/* Display name */}
          <div>
            <label htmlFor="displayName" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Display name
            </label>
            <input
              id="displayName"
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              disabled={isPending}
              aria-describedby={fieldErrors.displayName ? 'displayName-error' : undefined}
              className={clsx(
                'mt-1 block w-full rounded-lg border px-3 py-2 text-sm',
                'focus:outline-none focus:ring-2 focus:ring-primary-500',
                'dark:bg-gray-800 dark:text-white',
                fieldErrors.displayName
                  ? 'border-red-400 dark:border-red-500'
                  : 'border-gray-300 dark:border-gray-600',
                'disabled:opacity-50'
              )}
            />
            {fieldErrors.displayName && (
              <p id="displayName-error" role="alert" className="mt-1 text-xs text-red-500">
                {fieldErrors.displayName}
              </p>
            )}
          </div>

          {/* Bio */}
          <div>
            <label htmlFor="bio" className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Bio
            </label>
            <textarea
              id="bio"
              rows={3}
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              disabled={isPending}
              aria-describedby={fieldErrors.bio ? 'bio-error' : 'bio-count'}
              className={clsx(
                'mt-1 block w-full resize-none rounded-lg border px-3 py-2 text-sm',
                'focus:outline-none focus:ring-2 focus:ring-primary-500',
                'dark:bg-gray-800 dark:text-white',
                fieldErrors.bio
                  ? 'border-red-400 dark:border-red-500'
                  : 'border-gray-300 dark:border-gray-600',
                'disabled:opacity-50'
              )}
            />
            <p id="bio-count" className="mt-1 text-xs text-gray-400 dark:text-gray-500">
              {bio.length} / {BIO_MAX}
            </p>
            {fieldErrors.bio && (
              <p id="bio-error" role="alert" className="mt-1 text-xs text-red-500">
                {fieldErrors.bio}
              </p>
            )}
          </div>

          {errorMessage && (
            <div role="alert" className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-700 dark:bg-red-900/30 dark:text-red-300">
              {errorMessage}
            </div>
          )}
        </div>

        <div className="flex justify-end gap-3 border-t border-gray-200 px-6 py-4 dark:border-gray-700">
          <button
            onClick={onCancel}
            disabled={isPending}
            aria-label="Cancel editing"
            className={clsx(
              'rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700',
              'hover:bg-gray-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700',
              'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
              'disabled:opacity-50'
            )}
          >
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={isPending}
            aria-label="Save profile changes"
            className={clsx(
              'rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white',
              'hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2',
              'disabled:opacity-50',
              isPending && 'cursor-not-allowed'
            )}
          >
            {isPending ? 'Saving…' : 'Save'}
          </button>
        </div>
      </div>
    </div>
  )
}
