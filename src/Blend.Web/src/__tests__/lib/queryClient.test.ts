import { describe, it, expect } from 'vitest'
import { createQueryClient } from '@/lib/queryClient'

describe('queryClient', () => {
  it('should create a QueryClient instance', () => {
    const client = createQueryClient()
    expect(client).toBeDefined()
  })

  it('should have correct default query options', () => {
    const client = createQueryClient()
    const defaultOptions = client.getDefaultOptions()

    expect(defaultOptions.queries?.staleTime).toBe(60 * 1000)
    expect(defaultOptions.queries?.retry).toBe(2)
    expect(defaultOptions.queries?.refetchOnWindowFocus).toBe(false)
  })

  it('should have correct default mutation options', () => {
    const client = createQueryClient()
    const defaultOptions = client.getDefaultOptions()

    expect(defaultOptions.mutations?.retry).toBe(0)
  })

  it('should have gcTime set', () => {
    const client = createQueryClient()
    const defaultOptions = client.getDefaultOptions()

    expect(defaultOptions.queries?.gcTime).toBe(5 * 60 * 1000)
  })
})
