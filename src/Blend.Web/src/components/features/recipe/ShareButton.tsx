'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/Button'

export function ShareButton() {
  const [copied, setCopied] = useState(false)

  const handleShare = async () => {
    try {
      await navigator.clipboard.writeText(window.location.href)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // fallback: do nothing
    }
  }

  return (
    <Button variant="secondary" size="sm" onClick={handleShare} aria-label="Share recipe">
      {copied ? '✅ Copied!' : '🔗 Share'}
    </Button>
  )
}
