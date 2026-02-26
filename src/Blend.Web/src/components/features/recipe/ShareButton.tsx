'use client'

import { useState } from 'react'

export function ShareButton() {
  const [copied, setCopied] = useState(false)

  const handleShare = async () => {
    try {
      await navigator.clipboard.writeText(window.location.href)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // Clipboard API unavailable or permission denied; silently ignore
    }
  }

  return (
    <button
      aria-label="Share recipe"
      onClick={handleShare}
      className="flex items-center gap-2 rounded-full border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-50"
    >
      <span aria-hidden="true">↗</span>
      <span>{copied ? 'Copied!' : 'Share'}</span>
    </button>
  )
}
