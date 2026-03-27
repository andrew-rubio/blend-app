'use client'

import { useEffect } from 'react'
import { useSettingsStore } from '@/stores/settingsStore'

export function ThemeProvider() {
  const theme = useSettingsStore((s) => s.theme)

  useEffect(() => {
    const root = document.documentElement

    if (theme === 'dark') {
      root.classList.add('dark')
    } else if (theme === 'light') {
      root.classList.remove('dark')
    } else {
      // system: respect OS preference
      const mq = window.matchMedia('(prefers-color-scheme: dark)')
      const apply = () => {
        if (mq.matches) {
          root.classList.add('dark')
        } else {
          root.classList.remove('dark')
        }
      }
      apply()
      mq.addEventListener('change', apply)
      return () => mq.removeEventListener('change', apply)
    }
  }, [theme])

  return null
}
