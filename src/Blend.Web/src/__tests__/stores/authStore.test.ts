import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore } from '@/stores/authStore'
import type { User } from '@/types'

const mockUser: User = {
  id: '1',
  email: 'test@example.com',
  name: 'Test User',
  role: 'user',
  createdAt: '2024-01-01T00:00:00.000Z',
}

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.setState({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
    })
  })

  it('should have correct initial state', () => {
    const state = useAuthStore.getState()
    expect(state.user).toBeNull()
    expect(state.token).toBeNull()
    expect(state.isAuthenticated).toBe(false)
    expect(state.isLoading).toBe(false)
  })

  it('should login a user correctly', () => {
    const { login } = useAuthStore.getState()
    login(mockUser, 'test-token-123')

    const state = useAuthStore.getState()
    expect(state.user).toEqual(mockUser)
    expect(state.token).toBe('test-token-123')
    expect(state.isAuthenticated).toBe(true)
    expect(state.isLoading).toBe(false)
  })

  it('should logout a user correctly', () => {
    const store = useAuthStore.getState()
    store.login(mockUser, 'test-token-123')
    store.logout()

    const state = useAuthStore.getState()
    expect(state.user).toBeNull()
    expect(state.token).toBeNull()
    expect(state.isAuthenticated).toBe(false)
  })

  it('should set loading state', () => {
    const { setLoading } = useAuthStore.getState()
    setLoading(true)
    expect(useAuthStore.getState().isLoading).toBe(true)
    setLoading(false)
    expect(useAuthStore.getState().isLoading).toBe(false)
  })

  it('should update user details', () => {
    const store = useAuthStore.getState()
    store.login(mockUser, 'test-token-123')
    store.updateUser({ name: 'Updated Name' })

    const state = useAuthStore.getState()
    expect(state.user?.name).toBe('Updated Name')
    expect(state.user?.email).toBe(mockUser.email)
  })
})
