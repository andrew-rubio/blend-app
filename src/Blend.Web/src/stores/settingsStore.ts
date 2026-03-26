import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { UnitSystem } from '@/types'

interface SettingsState {
  unitSystem: UnitSystem
  /** ISO date string if a deletion has been requested, otherwise null. */
  pendingDeletionDate: string | null
}

interface SettingsActions {
  setUnitSystem: (unitSystem: UnitSystem) => void
  setPendingDeletionDate: (date: string | null) => void
}

type SettingsStore = SettingsState & SettingsActions

const initialState: SettingsState = {
  unitSystem: 'Metric',
  pendingDeletionDate: null,
}

export const useSettingsStore = create<SettingsStore>()(
  persist(
    (set) => ({
      ...initialState,
      setUnitSystem: (unitSystem: UnitSystem) => set({ unitSystem }),
      setPendingDeletionDate: (pendingDeletionDate: string | null) => set({ pendingDeletionDate }),
    }),
    {
      name: 'blend-settings',
      partialize: (state) => ({
        unitSystem: state.unitSystem,
        pendingDeletionDate: state.pendingDeletionDate,
      }),
    }
  )
)
