import type { Metadata } from 'next'
import { SettingsContainer } from '@/components/features/settings/SettingsContainer'

export const metadata: Metadata = {
  title: 'Settings',
}

export default function SettingsPage() {
  return <SettingsContainer />
}
