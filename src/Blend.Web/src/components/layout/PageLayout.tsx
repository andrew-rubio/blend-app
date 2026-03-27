import { Header } from './Header'
import { Footer } from './Footer'
import { BottomNav } from './BottomNav'
import type { ReactNode } from 'react'

interface PageLayoutProps {
  children: ReactNode
}

export function PageLayout({ children }: PageLayoutProps) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 pb-16 md:pb-0">{children}</main>
      <Footer className="hidden md:block" />
      <BottomNav />
    </div>
  )
}
