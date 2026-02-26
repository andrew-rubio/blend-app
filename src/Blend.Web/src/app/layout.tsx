import type { Metadata } from 'next'
import { QueryProvider } from '@/lib/query-client'
import './globals.css'

export const metadata: Metadata = {
  title: 'Blend - Recipe App',
  description: 'Discover and cook amazing recipes',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <QueryProvider>{children}</QueryProvider>
      </body>
    </html>
  )
}
