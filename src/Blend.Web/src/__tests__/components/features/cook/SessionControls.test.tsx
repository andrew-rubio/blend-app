import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { SessionControls } from '@/components/features/cook/SessionControls'

describe('SessionControls', () => {
  const mockOnPause = vi.fn()
  const mockOnFinish = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders Pause and Finish buttons', () => {
    render(<SessionControls onPause={mockOnPause} onFinish={mockOnFinish} isPausing={false} isFinishing={false} />)
    expect(screen.getByTestId('session-controls-pause')).toBeDefined()
    expect(screen.getByTestId('session-controls-finish')).toBeDefined()
  })

  it('calls onPause when pause button clicked', () => {
    render(<SessionControls onPause={mockOnPause} onFinish={mockOnFinish} isPausing={false} isFinishing={false} />)
    fireEvent.click(screen.getByTestId('session-controls-pause'))
    expect(mockOnPause).toHaveBeenCalled()
  })

  it('calls onFinish when finish button clicked', () => {
    render(<SessionControls onPause={mockOnPause} onFinish={mockOnFinish} isPausing={false} isFinishing={false} />)
    fireEvent.click(screen.getByTestId('session-controls-finish'))
    expect(mockOnFinish).toHaveBeenCalled()
  })

  it('disables buttons when isPausing is true', () => {
    render(<SessionControls onPause={mockOnPause} onFinish={mockOnFinish} isPausing={true} isFinishing={false} />)
    expect((screen.getByTestId('session-controls-pause') as HTMLButtonElement).disabled).toBe(true)
    expect((screen.getByTestId('session-controls-finish') as HTMLButtonElement).disabled).toBe(true)
  })

  it('disables buttons when isFinishing is true', () => {
    render(<SessionControls onPause={mockOnPause} onFinish={mockOnFinish} isPausing={false} isFinishing={true} />)
    expect((screen.getByTestId('session-controls-pause') as HTMLButtonElement).disabled).toBe(true)
    expect((screen.getByTestId('session-controls-finish') as HTMLButtonElement).disabled).toBe(true)
  })
})
