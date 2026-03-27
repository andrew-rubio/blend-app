import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { ThemeMode, UnitSystem } from '@/types'

interface SettingsState {
  unitSystem: UnitSystem
  theme: ThemeMode
  /** ISO date string if a deletion has been requested, otherwise null. */
  pendingDeletionDate: string | null
}

interface SettingsActions {
  setUnitSystem: (unitSystem: UnitSystem) => void
  setTheme: (theme: ThemeMode) => void
  setPendingDeletionDate: (date: string | null) => void
}

type SettingsStore = SettingsState & SettingsActions

const initialState: SettingsState = {
  unitSystem: 'Metric',
  theme: 'system',
  pendingDeletionDate: null,
}

export const useSettingsStore = create<SettingsStore>()(
  persist(
    (set) => ({
      ...initialState,
      setUnitSystem: (unitSystem: UnitSystem) => set({ unitSystem }),
      setTheme: (theme: ThemeMode) => set({ theme }),
      setPendingDeletionDate: (pendingDeletionDate: string | null) => set({ pendingDeletionDate }),
    }),
    {
      name: 'blend-settings',
      partialize: (state) => ({
        unitSystem: state.unitSystem,
        theme: state.theme,
        pendingDeletionDate: state.pendingDeletionDate,
      }),
    }
  )
)
