import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User } from '@/types'

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isLoading: boolean
}

interface AuthActions {
  login: (user: User, token: string) => void
  logout: () => void
  setLoading: (loading: boolean) => void
  updateUser: (user: Partial<User>) => void
  setToken: (token: string) => void
}

type AuthStore = AuthState & AuthActions

const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      ...initialState,
      login: (user: User, token: string) =>
        set({ user, token, isAuthenticated: true, isLoading: false }),
      logout: () => set({ ...initialState }),
      setLoading: (isLoading: boolean) => set({ isLoading }),
      updateUser: (updates: Partial<User>) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...updates } : null,
        })),
      setToken: (token: string) => set({ token }),
    }),
    {
      name: 'blend-auth',
      // Only persist non-sensitive state. JWT token is NOT persisted to prevent XSS token theft.
      // The token is held in memory only and refreshed via httpOnly cookie on page reload.
      partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
    }
  )
)
