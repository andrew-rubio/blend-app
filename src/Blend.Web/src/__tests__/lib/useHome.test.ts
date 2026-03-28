import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import React from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useHome, useRefreshHome, homeQueryKeys } from '@/hooks/useHome'
import { getHomeApi } from '@/lib/api/home'

vi.mock('@/lib/api/home', () => ({
  getHomeApi: vi.fn(),
}))

const mockHomeData = {
  search: { placeholder: 'Search...' },
  featured: { recipes: [], stories: [], videos: [] },
  community: { recipes: [] },
  recentlyViewed: { recipes: [] },
}

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  const Wrapper = ({ children }: { children: React.ReactNode }) => (
    React.createElement(QueryClientProvider, { client: queryClient }, children)
  )
  Wrapper.displayName = 'TestQueryWrapper'
  return Wrapper
}

describe('useHome', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetches home data successfully', async () => {
    vi.mocked(getHomeApi).mockResolvedValueOnce(mockHomeData)
    const { result } = renderHook(() => useHome(), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isSuccess).toBe(true))
    expect(result.current.data).toEqual(mockHomeData)
  })

  it('returns error on failure', async () => {
    vi.mocked(getHomeApi).mockRejectedValueOnce({ message: 'Failed', status: 500 })
    const { result } = renderHook(() => useHome(), { wrapper: createWrapper() })
    await waitFor(() => expect(result.current.isError).toBe(true))
  })
})

describe('homeQueryKeys', () => {
  it('has correct all key', () => {
    expect(homeQueryKeys.all).toEqual(['home'])
  })

  it('has correct data key', () => {
    expect(homeQueryKeys.data()).toEqual(['home', 'data'])
  })
})

describe('useRefreshHome', () => {
  it('returns a function', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const wrapper = ({ children }: { children: React.ReactNode }) =>
      React.createElement(QueryClientProvider, { client: queryClient }, children)
    const { result } = renderHook(() => useRefreshHome(), { wrapper })
    expect(typeof result.current).toBe('function')
  })
})
