import { PageLayout } from '@/components/layout/PageLayout'
import type { ReactNode } from 'react'

interface MainLayoutProps {
  children: ReactNode
}

export default function MainLayout({ children }: MainLayoutProps) {
  return <PageLayout>{children}</PageLayout>
}
