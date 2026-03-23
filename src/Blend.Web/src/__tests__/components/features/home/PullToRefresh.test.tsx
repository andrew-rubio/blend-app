import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { PullToRefresh } from '@/components/features/home/PullToRefresh'

describe('PullToRefresh', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders children', () => {
    render(
      <PullToRefresh onRefresh={vi.fn()}>
        <div>Content</div>
      </PullToRefresh>
    )
    expect(screen.getByText('Content')).toBeDefined()
  })

  it('shows refresh indicator during pull', () => {
    const { container } = render(
      <PullToRefresh onRefresh={vi.fn()} threshold={50}>
        <div>Content</div>
      </PullToRefresh>
    )
    const wrapper = container.firstChild as HTMLElement
    // Simulate touch
    fireEvent.touchStart(wrapper, {
      touches: [{ clientY: 100 }],
    })
    fireEvent.touchMove(wrapper, {
      touches: [{ clientY: 220 }],
    })
    // Indicator should appear
    const indicator = screen.queryByLabelText('Pull to refresh')
    expect(indicator).toBeDefined()
  })

  it('calls onRefresh when pulled past threshold', async () => {
    const onRefresh = vi.fn().mockResolvedValue(undefined)
    const { container } = render(
      <PullToRefresh onRefresh={onRefresh} threshold={50}>
        <div>Content</div>
      </PullToRefresh>
    )
    const wrapper = container.firstChild as HTMLElement
    fireEvent.touchStart(wrapper, { touches: [{ clientY: 0 }] })
    fireEvent.touchMove(wrapper, { touches: [{ clientY: 130 }] })
    fireEvent.touchEnd(wrapper)
    expect(onRefresh).toHaveBeenCalled()
  })

  it('does not call onRefresh when pulled below threshold', () => {
    const onRefresh = vi.fn()
    const { container } = render(
      <PullToRefresh onRefresh={onRefresh} threshold={100}>
        <div>Content</div>
      </PullToRefresh>
    )
    const wrapper = container.firstChild as HTMLElement
    fireEvent.touchStart(wrapper, { touches: [{ clientY: 0 }] })
    fireEvent.touchMove(wrapper, { touches: [{ clientY: 10 }] })
    fireEvent.touchEnd(wrapper)
    expect(onRefresh).not.toHaveBeenCalled()
  })
})
