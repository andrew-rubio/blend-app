'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { useAuthStore } from '@/lib/stores/auth-store'

interface CookButtonProps {
  recipeId: string
}

export function CookButton({ recipeId }: CookButtonProps) {
  const { isAuthenticated } = useAuthStore()
  const router = useRouter()
  const [showLoginPrompt, setShowLoginPrompt] = useState(false)

  const handleCook = () => {
    if (isAuthenticated) {
      router.push(`/cook/${recipeId}`)
    } else {
      setShowLoginPrompt(true)
    }
  }

  return (
    <>
      <Button variant="primary" size="md" onClick={handleCook}>
        🍳 Cook this dish
      </Button>

      {showLoginPrompt && (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="login-prompt-title"
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
        >
          <div className="mx-4 w-full max-w-sm rounded-2xl bg-white p-6 shadow-xl">
            <h2 id="login-prompt-title" className="text-xl font-semibold text-gray-900">
              Sign in to cook
            </h2>
            <p className="mt-2 text-gray-600">
              Create an account or sign in to start cooking this recipe.
            </p>
            <div className="mt-6 flex gap-3">
              <Button
                variant="primary"
                size="md"
                className="flex-1"
                onClick={() => router.push('/auth/login')}
              >
                Sign in
              </Button>
              <Button
                variant="secondary"
                size="md"
                className="flex-1"
                onClick={() => setShowLoginPrompt(false)}
              >
                Cancel
              </Button>
            </div>
          </div>
        </div>
      )}
    </>
  )
}
