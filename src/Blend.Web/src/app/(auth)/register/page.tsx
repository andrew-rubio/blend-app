import Link from 'next/link'
import { Card } from '@/components/ui/Card'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'Create account',
}

export default function RegisterPage() {
  return (
    <Card variant="elevated" padding="lg">
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">Create your account</h1>

      <form className="flex flex-col gap-4">
        <Input
          label="Full name"
          type="text"
          placeholder="Jane Doe"
          autoComplete="name"
          required
        />
        <Input
          label="Email address"
          type="email"
          placeholder="you@example.com"
          autoComplete="email"
          required
        />
        <Input
          label="Password"
          type="password"
          placeholder="••••••••"
          autoComplete="new-password"
          required
        />

        <Button type="submit" variant="primary" className="w-full">
          Create account
        </Button>
      </form>

      <p className="mt-4 text-center text-sm text-gray-600 dark:text-gray-400">
        Already have an account?{' '}
        <Link href="/login" className="font-medium text-primary-600 hover:text-primary-700">
          Sign in
        </Link>
      </p>
    </Card>
  )
}
