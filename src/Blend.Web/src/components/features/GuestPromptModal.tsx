'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/Button'

interface GuestPromptModalProps {
  isOpen: boolean
  onClose: () => void
  message?: string
}

export function GuestPromptModal({ isOpen, onClose, message }: GuestPromptModalProps) {
  if (!isOpen) return null

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Sign in required"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
      data-testid="guest-prompt-modal"
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose()
      }}
    >
      <div className="w-full max-w-sm rounded-2xl bg-white p-6 shadow-2xl dark:bg-gray-900">
        <div className="mb-4 text-center text-4xl" role="img" aria-label="lock">
          🔒
        </div>
        <h2 className="mb-2 text-center text-xl font-bold text-gray-900 dark:text-white">
          Sign in to continue
        </h2>
        <p className="mb-6 text-center text-sm text-gray-600 dark:text-gray-400">
          {message ?? 'Create a free account or sign in to access this feature.'}
        </p>

        <div className="flex flex-col gap-3">
          <Link href="/register" onClick={onClose} className="block">
            <Button variant="primary" className="w-full">
              Create a free account
            </Button>
          </Link>
          <Link href="/login" onClick={onClose} className="block">
            <Button variant="outline" className="w-full">
              Sign in
            </Button>
          </Link>
          <Button variant="ghost" className="w-full" onClick={onClose}>
            Maybe later
          </Button>
        </div>
      </div>
    </div>
  )
}

export function useGuestPrompt() {
  const [isOpen, setIsOpen] = useState(false)
  const [message, setMessage] = useState<string | undefined>()

  const prompt = (msg?: string) => {
    setMessage(msg)
    setIsOpen(true)
  }

  const close = () => setIsOpen(false)

  return { isOpen, message, prompt, close }
}
