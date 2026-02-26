'use client'

import { useEffect } from 'react'

interface ErrorProps {
  error: Error & { digest?: string }
  reset: () => void
}

export default function Error({ error, reset }: ErrorProps) {
  useEffect(() => {
    console.error(error)
  }, [error])

  return (
    <div className="flex min-h-screen flex-col items-center justify-center text-center">
      <h2 className="text-2xl font-semibold text-gray-900">Something went wrong</h2>
      <button
        onClick={reset}
        className="mt-6 rounded-lg bg-orange-500 px-6 py-2 text-white hover:bg-orange-600"
      >
        Try again
      </button>
    </div>
  )
}
