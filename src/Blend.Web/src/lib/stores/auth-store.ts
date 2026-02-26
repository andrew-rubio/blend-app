import { create } from 'zustand'

interface AuthState {
  isAuthenticated: boolean
  user: { id: string; name: string; avatarUrl?: string } | null
  setUser: (user: AuthState['user']) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: false,
  user: null,
  setUser: (user) => set({ user, isAuthenticated: user !== null }),
  logout: () => set({ user: null, isAuthenticated: false }),
}))
