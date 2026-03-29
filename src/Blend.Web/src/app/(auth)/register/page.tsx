'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { useAuthStore } from '@/stores/authStore'
import { registerApi, getSocialLoginUrl } from '@/lib/api/auth'
import type { ApiErrorData } from '@/lib/api/auth'
import {
  checkPasswordRequirements,
  getPasswordStrength,
  isPasswordValid,
  strengthColors,
  strengthLabels,
  strengthWidths,
} from '@/lib/passwordStrength'

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

interface PasswordStrengthMeterProps {
  password: string
}

function PasswordStrengthMeter({ password }: PasswordStrengthMeterProps) {
  if (!password) return null

  const strength = getPasswordStrength(password)
  const reqs = checkPasswordRequirements(password)

  return (
    <div className="mt-2 space-y-2">
      <div className="flex items-center gap-2">
        <div className="h-1.5 flex-1 rounded-full bg-gray-200 dark:bg-gray-700">
          <div
            className={`h-1.5 rounded-full transition-all ${strengthColors[strength]} ${strengthWidths[strength]}`}
          />
        </div>
        <span className="text-xs text-gray-500 dark:text-gray-400">{strengthLabels[strength]}</span>
      </div>
      <ul className="space-y-1 text-xs" aria-label="Password requirements">
        <li className={reqs.minLength ? 'text-green-600 dark:text-green-400' : 'text-gray-500 dark:text-gray-400'}>
          {reqs.minLength ? '✓' : '○'} At least 8 characters
        </li>
        <li className={reqs.hasUppercase ? 'text-green-600 dark:text-green-400' : 'text-gray-500 dark:text-gray-400'}>
          {reqs.hasUppercase ? '✓' : '○'} One uppercase letter
        </li>
        <li className={reqs.hasLowercase ? 'text-green-600 dark:text-green-400' : 'text-gray-500 dark:text-gray-400'}>
          {reqs.hasLowercase ? '✓' : '○'} One lowercase letter
        </li>
        <li className={reqs.hasNumber ? 'text-green-600 dark:text-green-400' : 'text-gray-500 dark:text-gray-400'}>
          {reqs.hasNumber ? '✓' : '○'} One number
        </li>
        <li className={reqs.hasSpecial ? 'text-green-600 dark:text-green-400' : 'text-gray-500 dark:text-gray-400'}>
          {reqs.hasSpecial ? '✓' : '○'} One special character
        </li>
      </ul>
    </div>
  )
}

export default function RegisterPage() {
  const router = useRouter()
  const { login } = useAuthStore()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [errors, setErrors] = useState<{
    name?: string
    email?: string
    password?: string
    confirmPassword?: string
    form?: string
  }>({})
  const [isLoading, setIsLoading] = useState(false)

  const validate = (): boolean => {
    const newErrors: typeof errors = {}
    if (!name.trim()) newErrors.name = 'Display name is required'
    const emailError = validateEmail(email)
    if (emailError) newErrors.email = emailError
    if (!isPasswordValid(password)) {
      newErrors.password = 'Password does not meet the requirements'
    }
    if (!confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password'
    } else if (password !== confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match'
    }
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!validate()) return

    setIsLoading(true)
    setErrors({})
    try {
      const response = await registerApi({ displayName: name.trim(), email, password })
      login(response.user, response.token)
      router.push('/preferences')
    } catch (err: unknown) {
      const apiErr = err as Partial<ApiErrorData>
      setErrors({ form: apiErr.message ?? 'Registration failed. Please try again.' })
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Card variant="elevated" padding="lg">
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">Create your account</h1>

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
          label="Display name"
          type="text"
          placeholder="Jane Doe"
          autoComplete="name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          error={errors.name}
        />
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
            autoComplete="new-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={errors.password}
          />
          <PasswordStrengthMeter password={password} />
        </div>
        <Input
          label="Confirm password"
          type="password"
          placeholder="••••••••"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          error={errors.confirmPassword}
        />

        <Button type="submit" variant="primary" className="w-full" isLoading={isLoading}>
          Create account
        </Button>

        <Button
          type="button"
          variant="ghost"
          className="w-full"
          onClick={() => router.push('/home')}
        >
          Skip preferences — go straight to Home
        </Button>
      </form>

      <div className="my-4 flex items-center gap-3">
        <div className="flex-1 border-t border-gray-200 dark:border-gray-700" />
        <span className="text-sm text-gray-500 dark:text-gray-400">or continue with</span>
        <div className="flex-1 border-t border-gray-200 dark:border-gray-700" />
      </div>

      <div className="flex flex-col gap-3">
        <SocialLoginButton provider="google" label="Sign up with Google" />
        <SocialLoginButton provider="facebook" label="Sign up with Facebook" />
        <SocialLoginButton provider="twitter" label="Sign up with Twitter / X" />
      </div>

      <p className="mt-4 text-center text-sm text-gray-600 dark:text-gray-400">
        Already have an account?{' '}
        <Link href="/login" className="font-medium text-primary-600 hover:text-primary-700">
          Sign in
        </Link>
      </p>
    </Card>
  )
}
