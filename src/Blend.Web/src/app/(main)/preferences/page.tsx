import type { Metadata } from 'next'
import { PreferenceWizard } from '@/components/features/preferences/PreferenceWizard'

export const metadata: Metadata = {
  title: 'Set Up Your Preferences',
}

export default function PreferencesPage() {
  return <PreferenceWizard />
}
