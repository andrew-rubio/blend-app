'use client'

import { clsx } from 'clsx'
import { useSettingsStore } from '@/stores/settingsStore'
import { useUpdateSettings } from '@/hooks/useSettings'
import type { UnitSystem } from '@/types'

const UNIT_OPTIONS: { value: UnitSystem; label: string }[] = [
  { value: 'Metric', label: 'Metric' },
  { value: 'Imperial', label: 'Imperial' },
]

/**
 * Segmented control that switches between Metric and Imperial units (SETT-13 through SETT-16).
 * Applies immediately via the Zustand store and persists via the API.
 */
export function UnitToggle() {
  const unitSystem = useSettingsStore((s) => s.unitSystem)
  const { mutate: updateSettings, isPending } = useUpdateSettings()

  function handleSelect(value: UnitSystem) {
    if (value === unitSystem) return
    updateSettings({ unitSystem: value })
  }

  return (
    <div className="flex items-center gap-2" role="group" aria-label="Unit system">
      {UNIT_OPTIONS.map(({ value, label }) => (
        <button
          key={value}
          type="button"
          onClick={() => handleSelect(value)}
          disabled={isPending}
          aria-pressed={unitSystem === value}
          className={clsx(
            'rounded-lg px-4 py-2 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 disabled:opacity-50',
            unitSystem === value
              ? 'bg-primary-600 text-white'
              : 'border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700'
          )}
        >
          {label}
        </button>
      ))}
    </div>
  )
}
