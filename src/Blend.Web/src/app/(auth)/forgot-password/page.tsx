'use client'

import { useState } from 'react'
import Link from 'next/link'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { forgotPasswordApi } from '@/lib/api/auth'

function validateEmail(email: string): string | undefined {
  if (!email) return 'Email is required'
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Invalid email format'
  return undefined
}

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [emailError, setEmailError] = useState<string | undefined>()
  const [isLoading, setIsLoading] = useState(false)
  const [submitted, setSubmitted] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const err = validateEmail(email)
    setEmailError(err)
    if (err) return

    setIsLoading(true)
    try {
      await forgotPasswordApi(email)
    } catch {
      // always show generic success message regardless of backend response (AUTH-21)
    } finally {
      setIsLoading(false)
      setSubmitted(true)
    }
  }

  return (
    <Card variant="elevated" padding="lg">
      <h1 className="mb-2 text-2xl font-bold text-gray-900 dark:text-white">Reset your password</h1>
      <p className="mb-6 text-sm text-gray-600 dark:text-gray-400">
        Enter the email address for your account and we&apos;ll send you a password reset link.
      </p>

      {submitted ? (
        <div
          role="status"
          className="rounded-md bg-green-50 p-4 text-sm text-green-700 dark:bg-green-900/20 dark:text-green-400"
        >
          <p className="font-medium">Check your email</p>
          <p className="mt-1">
            If an account exists for <strong>{email}</strong>, you will receive a password reset
            link shortly.
          </p>
        </div>
      ) : (
        <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-4">
          <Input
            label="Email address"
            type="email"
            placeholder="you@example.com"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={emailError}
          />
          <Button type="submit" variant="primary" className="w-full" isLoading={isLoading}>
            Send reset link
          </Button>
        </form>
      )}

      <p className="mt-4 text-center text-sm text-gray-600 dark:text-gray-400">
        Remembered your password?{' '}
        <Link href="/login" className="font-medium text-primary-600 hover:text-primary-700">
          Sign in
        </Link>
      </p>
    </Card>
  )
}
