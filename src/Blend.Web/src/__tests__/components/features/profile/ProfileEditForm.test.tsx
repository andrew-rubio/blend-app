import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { ProfileEditForm } from '@/components/features/profile/ProfileEditForm'
import type { MyProfile } from '@/types'

vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

vi.mock('@/hooks/useProfile', () => ({
  useUpdateProfile: vi.fn(),
  useAvatarUpload: vi.fn(),
}))

import { useUpdateProfile, useAvatarUpload } from '@/hooks/useProfile'

const mockUseUpdateProfile = vi.mocked(useUpdateProfile)
const mockUseAvatarUpload = vi.mocked(useAvatarUpload)

const mockProfile: MyProfile = {
  id: 'user1',
  displayName: 'Alice Chef',
  email: 'alice@example.com',
  bio: 'Cook',
  joinDate: '2022-01-15T00:00:00Z',
  recipeCount: 10,
  likeCount: 250,
  followerCount: 80,
  followingCount: 45,
}

describe('ProfileEditForm', () => {
  const mockUpdateMutate = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
    mockUseUpdateProfile.mockReturnValue({
      mutate: mockUpdateMutate,
      isPending: false,
      error: null,
    } as unknown as ReturnType<typeof useUpdateProfile>)
    mockUseAvatarUpload.mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as unknown as ReturnType<typeof useAvatarUpload>)
  })

  it('renders edit profile dialog', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    expect(screen.getByLabelText('Edit profile')).toBeDefined()
  })

  it('pre-fills display name field', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    const input = screen.getByLabelText('Display name') as HTMLInputElement
    expect(input.value).toBe('Alice Chef')
  })

  it('pre-fills bio field', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    const textarea = screen.getByLabelText('Bio') as HTMLTextAreaElement
    expect(textarea.value).toBe('Cook')
  })

  it('calls onCancel when cancel button is clicked', () => {
    const onCancel = vi.fn()
    render(<ProfileEditForm profile={mockProfile} onCancel={onCancel} onSaved={vi.fn()} />)
    fireEvent.click(screen.getByLabelText('Cancel editing'))
    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('shows validation error for short display name', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    const input = screen.getByLabelText('Display name') as HTMLInputElement
    fireEvent.change(input, { target: { value: 'A' } })
    fireEvent.click(screen.getByLabelText('Save profile changes'))
    expect(screen.getByText(/Display name must be between/)).toBeDefined()
    expect(mockUpdateMutate).not.toHaveBeenCalled()
  })

  it('shows validation error for bio exceeding 500 chars', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    const textarea = screen.getByLabelText('Bio') as HTMLTextAreaElement
    fireEvent.change(textarea, { target: { value: 'x'.repeat(501) } })
    fireEvent.click(screen.getByLabelText('Save profile changes'))
    expect(screen.getByText(/Bio must be at most 500/)).toBeDefined()
  })

  it('calls updateProfile when form is valid and save clicked', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    fireEvent.click(screen.getByLabelText('Save profile changes'))
    expect(mockUpdateMutate).toHaveBeenCalledWith(
      expect.objectContaining({ displayName: 'Alice Chef' }),
      expect.any(Object)
    )
  })

  it('renders avatar upload button', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    expect(screen.getByText('Change photo')).toBeDefined()
  })

  it('shows close button', () => {
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    expect(screen.getByLabelText('Close edit form')).toBeDefined()
  })

  it('shows save error message when update fails', () => {
    mockUseUpdateProfile.mockReturnValue({
      mutate: mockUpdateMutate,
      isPending: false,
      error: { message: 'Failed to save', status: 500 },
    } as unknown as ReturnType<typeof useUpdateProfile>)
    render(<ProfileEditForm profile={mockProfile} onCancel={vi.fn()} onSaved={vi.fn()} />)
    expect(screen.getByText('Failed to save')).toBeDefined()
  })
})
