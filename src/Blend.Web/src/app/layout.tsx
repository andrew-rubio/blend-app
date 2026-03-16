import type { Metadata } from 'next'
import { Providers } from '@/lib/providers'
import './globals.css'
import type { ReactNode } from 'react'

export const metadata: Metadata = {
  title: {
    default: 'Blend',
    template: '%s | Blend',
  },
  description: 'Discover and share amazing recipes with Blend',
}

interface RootLayoutProps {
  children: ReactNode
}

export default function RootLayout({ children }: RootLayoutProps) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="font-sans antialiased">
        <Providers>{children}</Providers>
      </body>
    </html>
  )
}
