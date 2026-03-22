'use client'

import { useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Suspense } from 'react'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { resetPasswordApi } from '@/lib/api/auth'
import type { ApiErrorData } from '@/lib/api/auth'
import {
  checkPasswordRequirements,
  getPasswordStrength,
  isPasswordValid,
  strengthColors,
  strengthLabels,
  strengthWidths,
} from '@/lib/passwordStrength'

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

function ResetPasswordForm() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const token = searchParams.get('token')

  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [errors, setErrors] = useState<{
    password?: string
    confirmPassword?: string
    form?: string
  }>({})
  const [isLoading, setIsLoading] = useState(false)
  const [success, setSuccess] = useState(false)

  if (!token) {
    return (
      <div
        role="alert"
        className="rounded-md bg-red-50 p-4 text-sm text-red-700 dark:bg-red-900/20 dark:text-red-400"
      >
        <p className="font-medium">Invalid reset link</p>
        <p className="mt-1">
          This password reset link is invalid or has expired. Please{' '}
          <a href="/forgot-password" className="underline">
            request a new one
          </a>
          .
        </p>
      </div>
    )
  }

  const validate = (): boolean => {
    const newErrors: typeof errors = {}
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
      await resetPasswordApi(token, password)
      setSuccess(true)
      setTimeout(() => router.push('/login'), 3000)
    } catch (err: unknown) {
      const apiErr = err as Partial<ApiErrorData>
      if (apiErr.status === 400 || apiErr.status === 422) {
        setErrors({ form: 'This reset link is invalid or has expired. Please request a new one.' })
      } else {
        setErrors({ form: apiErr.message ?? 'Failed to reset password. Please try again.' })
      }
    } finally {
      setIsLoading(false)
    }
  }

  if (success) {
    return (
      <div
        role="status"
        className="rounded-md bg-green-50 p-4 text-sm text-green-700 dark:bg-green-900/20 dark:text-green-400"
      >
        <p className="font-medium">Password reset successfully!</p>
        <p className="mt-1">Your password has been updated. Redirecting you to sign in&hellip;</p>
      </div>
    )
  }

  return (
    <>
      {errors.form && (
        <div
          role="alert"
          className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-600 dark:bg-red-900/20 dark:text-red-400"
        >
          {errors.form}
        </div>
      )}
      <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-4">
        <div>
          <Input
            label="New password"
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
          label="Confirm new password"
          type="password"
          placeholder="••••••••"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          error={errors.confirmPassword}
        />
        <Button type="submit" variant="primary" className="w-full" isLoading={isLoading}>
          Reset password
        </Button>
      </form>
    </>
  )
}

export default function ResetPasswordPage() {
  return (
    <Card variant="elevated" padding="lg">
      <h1 className="mb-2 text-2xl font-bold text-gray-900 dark:text-white">Set new password</h1>
      <p className="mb-6 text-sm text-gray-600 dark:text-gray-400">
        Choose a strong password for your account.
      </p>
      <Suspense fallback={<p className="text-sm text-gray-500">Loading&hellip;</p>}>
        <ResetPasswordForm />
      </Suspense>
    </Card>
  )
}
