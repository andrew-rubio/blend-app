'use client';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getPreferences, savePreferences } from '@/lib/api/preferences';
import { usePreferencesStore } from '@/lib/stores/preferences-store';
import type { UserPreferences } from '@/types/preferences';

export const PREFERENCES_QUERY_KEY = ['preferences'] as const;

export function usePreferences() {
  return useQuery({
    queryKey: PREFERENCES_QUERY_KEY,
    queryFn: getPreferences,
  });
}

export function useSavePreferences() {
  const queryClient = useQueryClient();
  const setPreferences = usePreferencesStore((s) => s.setPreferences);

  return useMutation({
    mutationFn: savePreferences,
    onSuccess: (_: void, variables: UserPreferences) => {
      setPreferences(variables);
      queryClient.invalidateQueries({ queryKey: PREFERENCES_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['search'] });
      queryClient.invalidateQueries({ queryKey: ['home'] });
    },
  });
}
