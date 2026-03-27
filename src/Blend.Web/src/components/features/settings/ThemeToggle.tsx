'use client'

import { useSettingsStore } from '@/stores/settingsStore'
import { useUpdateSettings } from '@/hooks/useSettings'
import type { ThemeMode } from '@/types'

const options: { value: ThemeMode; label: string }[] = [
  { value: 'system', label: 'System' },
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
]

export function ThemeToggle() {
  const theme = useSettingsStore((s) => s.theme)
  const updateSettings = useUpdateSettings()

  function handleChange(value: ThemeMode) {
    updateSettings.mutate({ theme: value })
  }

  return (
    <div className="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-700" role="radiogroup" aria-label="Theme">
      {options.map((opt) => (
        <button
          key={opt.value}
          type="button"
          role="radio"
          aria-checked={theme === opt.value}
          onClick={() => handleChange(opt.value)}
          className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
            theme === opt.value
              ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-600 dark:text-white'
              : 'text-gray-500 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white'
          }`}
          data-testid={`theme-toggle-${opt.value}`}
        >
          {opt.label}
        </button>
      ))}
    </div>
  )
}
