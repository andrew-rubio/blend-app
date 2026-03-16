import Link from 'next/link'
import { Button } from '@/components/ui/Button'

export default function NotFoundPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center">
      <div className="mx-auto max-w-md text-center">
        <h1 className="mb-4 text-8xl font-bold text-primary-600">404</h1>
        <h2 className="mb-4 text-2xl font-semibold text-gray-900 dark:text-white">
          Page not found
        </h2>
        <p className="mb-8 text-gray-600 dark:text-gray-400">
          The page you&apos;re looking for doesn&apos;t exist or has been moved.
        </p>
        <Link href="/">
          <Button variant="primary">Return home</Button>
        </Link>
      </div>
    </div>
  )
}
