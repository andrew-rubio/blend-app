'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuthStore } from '@/lib/stores/auth-store'

interface Props { recipeId: string }

export function CookButton({ recipeId }: Props) {
  const { isAuthenticated } = useAuthStore()
  const router = useRouter()
  const [showPrompt, setShowPrompt] = useState(false)

  const handleCook = () => {
    if (isAuthenticated) {
      router.push(`/cook/${recipeId}`)
    } else {
      setShowPrompt(true)
    }
  }

  return (
    <>
      <button
        aria-label="Cook this dish"
        onClick={handleCook}
        className="rounded-full bg-orange-500 px-6 py-2 text-sm font-semibold text-white hover:bg-orange-600"
      >
        Cook this dish
      </button>
      {showPrompt && (
        <div
          role="dialog"
          aria-modal="true"
          aria-label="Login required"
          className="fixed inset-0 flex items-center justify-center bg-black/50 p-4 z-50"
          onClick={() => setShowPrompt(false)}
        >
          <div
            className="w-full max-w-sm rounded-2xl bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 className="text-lg font-semibold text-gray-900">Sign in to cook</h2>
            <p className="mt-2 text-sm text-gray-500">You need an account to enter Cook Mode.</p>
            <div className="mt-6 flex gap-3">
              <a
                href="/auth/login"
                className="flex-1 rounded-lg bg-orange-500 py-2 text-center text-sm font-semibold text-white hover:bg-orange-600"
              >
                Sign in
              </a>
              <button
                onClick={() => setShowPrompt(false)}
                className="flex-1 rounded-lg border border-gray-300 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  )
}
