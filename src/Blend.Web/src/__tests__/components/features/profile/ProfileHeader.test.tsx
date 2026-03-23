import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { ProfileHeader } from '@/components/features/profile/ProfileHeader'
import type { MyProfile } from '@/types'

vi.mock('next/image', () => ({
  default: ({ src, alt }: { src: string; alt: string }) => <img src={src} alt={alt} />,
}))

const mockProfile: MyProfile = {
  id: 'user1',
  displayName: 'Alice Chef',
  email: 'alice@example.com',
  avatarUrl: 'https://example.com/avatar.jpg',
  bio: 'I love cooking Italian food.',
  joinDate: '2022-01-15T00:00:00Z',
  recipeCount: 10,
  likeCount: 250,
  followerCount: 80,
  followingCount: 45,
}

describe('ProfileHeader', () => {
  it('renders display name', () => {
    render(<ProfileHeader profile={mockProfile} onEditClick={vi.fn()} />)
    expect(screen.getByText('Alice Chef')).toBeDefined()
  })

  it('renders bio', () => {
    render(<ProfileHeader profile={mockProfile} onEditClick={vi.fn()} />)
    expect(screen.getByText('I love cooking Italian food.')).toBeDefined()
  })

  it('renders join year', () => {
    render(<ProfileHeader profile={mockProfile} onEditClick={vi.fn()} />)
    expect(screen.getByText(/Member since 2022/)).toBeDefined()
  })

  it('renders stats bar', () => {
    render(<ProfileHeader profile={mockProfile} onEditClick={vi.fn()} />)
    const statsBar = screen.getByLabelText('Profile stats')
    expect(statsBar).toBeDefined()
    expect(screen.getByText('10')).toBeDefined()
    expect(screen.getByText('250')).toBeDefined()
  })

  it('renders avatar image when avatarUrl is provided', () => {
    render(<ProfileHeader profile={mockProfile} onEditClick={vi.fn()} />)
    const img = screen.getByAltText('Alice Chef avatar')
    expect(img).toBeDefined()
  })

  it('does not render bio when bio is empty', () => {
    render(<ProfileHeader profile={{ ...mockProfile, bio: undefined }} onEditClick={vi.fn()} />)
    expect(screen.queryByText('I love cooking Italian food.')).toBeNull()
  })

  it('calls onEditClick when edit button is clicked', () => {
    const onEdit = vi.fn()
    render(<ProfileHeader profile={mockProfile} onEditClick={onEdit} />)
    fireEvent.click(screen.getByLabelText('Edit profile'))
    expect(onEdit).toHaveBeenCalledOnce()
  })

  it('renders edit profile button', () => {
    render(<ProfileHeader profile={mockProfile} onEditClick={vi.fn()} />)
    expect(screen.getByLabelText('Edit profile')).toBeDefined()
  })
})
