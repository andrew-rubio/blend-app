'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { useAuthStore } from '@/stores/authStore'
import { loginApi, getSocialLoginUrl } from '@/lib/api/auth'
import type { ApiErrorData } from '@/lib/api/auth'

function validateEmail(email: string): string | undefined {
  if (!email) return 'Email is required'
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Invalid email format'
  return undefined
}

interface SocialButtonProps {
  provider: 'google' | 'facebook' | 'twitter'
  label: string
}

function SocialLoginButton({ provider, label }: SocialButtonProps) {
  return (
    <a
      href={getSocialLoginUrl(provider)}
      className="inline-flex w-full items-center justify-center gap-2 rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
      data-testid={`social-login-${provider}`}
    >
      {label}
    </a>
  )
}

export default function LoginPage() {
  const router = useRouter()
  const { login } = useAuthStore()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<{ email?: string; password?: string; form?: string }>({})
  const [isLoading, setIsLoading] = useState(false)

  const validate = (): boolean => {
    const newErrors: typeof errors = {}
    const emailError = validateEmail(email)
    if (emailError) newErrors.email = emailError
    if (!password) newErrors.password = 'Password is required'
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!validate()) return

    setIsLoading(true)
    setErrors({})
    try {
      const response = await loginApi({ email, password })
      login(response.user, response.token)
      router.push('/home')
    } catch (err: unknown) {
      const apiErr = err as Partial<ApiErrorData>
      setErrors({ form: apiErr.message ?? 'Invalid credentials. Please try again.' })
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Card variant="elevated" padding="lg">
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">Sign in to Blend</h1>

      {errors.form && (
        <div
          role="alert"
          className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-600 dark:bg-red-900/20 dark:text-red-400"
        >
          {errors.form}
        </div>
      )}

      <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-4">
        <Input
          label="Email address"
          type="email"
          placeholder="you@example.com"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          error={errors.email}
        />
        <div>
          <Input
            label="Password"
            type="password"
            placeholder="••••••••"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={errors.password}
          />
          <div className="mt-1 text-right">
            <Link
              href="/forgot-password"
              className="text-sm text-primary-600 hover:text-primary-700"
            >
              Forgot your password?
            </Link>
          </div>
        </div>

        <Button type="submit" variant="primary" className="w-full" isLoading={isLoading}>
          Sign in
        </Button>
      </form>

      <div className="my-4 flex items-center gap-3">
        <div className="flex-1 border-t border-gray-200 dark:border-gray-700" />
        <span className="text-sm text-gray-500 dark:text-gray-400">or continue with</span>
        <div className="flex-1 border-t border-gray-200 dark:border-gray-700" />
      </div>

      <div className="flex flex-col gap-3">
        <SocialLoginButton provider="google" label="Continue with Google" />
        <SocialLoginButton provider="facebook" label="Continue with Facebook" />
        <SocialLoginButton provider="twitter" label="Continue with Twitter / X" />
      </div>

      <p className="mt-4 text-center text-sm text-gray-600 dark:text-gray-400">
        Don&apos;t have an account?{' '}
        <Link href="/register" className="font-medium text-primary-600 hover:text-primary-700">
          Sign up
        </Link>
      </p>
    </Card>
  )
}
