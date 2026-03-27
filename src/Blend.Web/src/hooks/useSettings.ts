import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getSettingsApi, updateSettingsApi, requestAccountDeletionApi, cancelAccountDeletionApi } from '@/lib/api/settings'
import { useSettingsStore } from '@/stores/settingsStore'
import type { UpdateSettingsRequest } from '@/types'

export const settingsQueryKeys = {
  all: ['settings'] as const,
  appSettings: () => [...settingsQueryKeys.all, 'app'] as const,
}

/**
 * Fetches the current user's app settings and syncs them into the Zustand store.
 */
export function useAppSettings() {
  const setUnitSystem = useSettingsStore((s) => s.setUnitSystem)
  const setTheme = useSettingsStore((s) => s.setTheme)

  return useQuery({
    queryKey: settingsQueryKeys.appSettings(),
    queryFn: async () => {
      const settings = await getSettingsApi()
      setUnitSystem(settings.unitSystem)
      if (settings.theme) setTheme(settings.theme)
      return settings
    },
  })
}

/**
 * Saves the user's app settings to the API and updates the local Zustand store.
 */
export function useUpdateSettings() {
  const queryClient = useQueryClient()
  const setUnitSystem = useSettingsStore((s) => s.setUnitSystem)
  const setTheme = useSettingsStore((s) => s.setTheme)

  return useMutation({
    mutationFn: (data: UpdateSettingsRequest) => updateSettingsApi(data),
    onSuccess: (updated) => {
      setUnitSystem(updated.unitSystem)
      if (updated.theme) setTheme(updated.theme)
      void queryClient.invalidateQueries({ queryKey: settingsQueryKeys.appSettings() })
    },
  })
}

/**
 * Submits an account deletion request.
 */
export function useRequestAccountDeletion() {
  const setPendingDeletionDate = useSettingsStore((s) => s.setPendingDeletionDate)

  return useMutation({
    mutationFn: (data: { password?: string }) => requestAccountDeletionApi(data),
    onSuccess: (result) => {
      setPendingDeletionDate(result.scheduledDeletionDate)
    },
  })
}

/**
 * Cancels a pending account deletion request.
 */
export function useCancelAccountDeletion() {
  const setPendingDeletionDate = useSettingsStore((s) => s.setPendingDeletionDate)

  return useMutation({
    mutationFn: () => cancelAccountDeletionApi(),
    onSuccess: () => {
      setPendingDeletionDate(null)
    },
  })
}
