import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { PhotoUploadStep } from '@/components/features/cook/wrap-up/PhotoUploadStep'

describe('PhotoUploadStep', () => {
  const defaultProps = {
    photos: [] as string[],
    primaryPhotoIndex: 0,
    onAdd: vi.fn(),
    onRemove: vi.fn(),
    onSetPrimary: vi.fn(),
    onNext: vi.fn(),
    onSkip: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders heading', () => {
    render(<PhotoUploadStep {...defaultProps} />)
    expect(screen.getByText('Upload Photos')).toBeDefined()
  })

  it('shows upload area when no photos', () => {
    render(<PhotoUploadStep {...defaultProps} />)
    expect(screen.getByTestId('photo-upload-area')).toBeDefined()
  })

  it('shows 0/5 photos count', () => {
    render(<PhotoUploadStep {...defaultProps} />)
    expect(screen.getByText('0/5 photos added')).toBeDefined()
  })

  it('renders photos when provided', () => {
    render(
      <PhotoUploadStep
        {...defaultProps}
        photos={['https://example.com/photo1.jpg', 'https://example.com/photo2.jpg']}
      />,
    )
    expect(screen.getByTestId('photo-0')).toBeDefined()
    expect(screen.getByTestId('photo-1')).toBeDefined()
  })

  it('hides upload area when 5 photos are added', () => {
    const fivePhotos = Array.from({ length: 5 }, (_, i) => `https://example.com/${i}.jpg`)
    render(<PhotoUploadStep {...defaultProps} photos={fivePhotos} />)
    expect(screen.queryByTestId('photo-upload-area')).toBeNull()
  })

  it('calls onRemove when remove button is clicked', () => {
    const onRemove = vi.fn()
    render(
      <PhotoUploadStep
        {...defaultProps}
        photos={['https://example.com/photo.jpg']}
        onRemove={onRemove}
      />,
    )
    fireEvent.click(screen.getByTestId('remove-photo-0'))
    expect(onRemove).toHaveBeenCalledWith(0)
  })

  it('calls onSetPrimary when set-primary button is clicked', () => {
    const onSetPrimary = vi.fn()
    render(
      <PhotoUploadStep
        {...defaultProps}
        photos={['https://example.com/photo.jpg', 'https://example.com/photo2.jpg']}
        onSetPrimary={onSetPrimary}
      />,
    )
    fireEvent.click(screen.getByTestId('set-primary-1'))
    expect(onSetPrimary).toHaveBeenCalledWith(1)
  })

  it('calls onSkip when Skip is clicked', () => {
    const onSkip = vi.fn()
    render(<PhotoUploadStep {...defaultProps} onSkip={onSkip} />)
    fireEvent.click(screen.getByTestId('photos-skip-button'))
    expect(onSkip).toHaveBeenCalledOnce()
  })

  it('calls onNext when Continue is clicked', () => {
    const onNext = vi.fn()
    render(<PhotoUploadStep {...defaultProps} onNext={onNext} />)
    fireEvent.click(screen.getByTestId('photos-next-button'))
    expect(onNext).toHaveBeenCalledOnce()
  })
})
