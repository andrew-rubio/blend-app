'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useSearchParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { PasswordStrengthIndicator } from '@/components/features/PasswordStrengthIndicator';
import { authApi } from '@/lib/apiClient';

const schema = z
  .object({
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/[A-Z]/, 'Must contain an uppercase letter')
      .regex(/[a-z]/, 'Must contain a lowercase letter')
      .regex(/[0-9]/, 'Must contain a number'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type FormData = z.infer<typeof schema>;

export function ResetPasswordForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token');
  const [serverError, setServerError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [password, setPassword] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  if (!token) {
    return (
      <div className="rounded-2xl bg-white p-8 shadow-sm text-center">
        <div className="mb-4 text-5xl">❌</div>
        <h2 className="mb-2 text-2xl font-bold text-gray-900">Invalid link</h2>
        <p className="mb-6 text-gray-500">
          This password reset link is invalid or has expired. Please request a new one.
        </p>
        <Link href="/forgot-password">
          <Button size="lg" className="w-full">
            Request new link
          </Button>
        </Link>
      </div>
    );
  }

  if (success) {
    return (
      <div className="rounded-2xl bg-white p-8 shadow-sm text-center">
        <div className="mb-4 text-5xl">✅</div>
        <h2 className="mb-2 text-2xl font-bold text-gray-900">Password updated</h2>
        <p className="mb-6 text-gray-500">Your password has been successfully reset.</p>
        <Button onClick={() => router.push('/login')} size="lg" className="w-full">
          Log in
        </Button>
      </div>
    );
  }

  const onSubmit = async (data: FormData) => {
    setServerError(null);
    try {
      await authApi.resetPassword(token, data.password);
      setSuccess(true);
    } catch {
      setServerError('This reset link is invalid or has expired. Please request a new one.');
    }
  };

  return (
    <div className="rounded-2xl bg-white p-8 shadow-sm">
      <h2 className="mb-2 text-2xl font-bold text-gray-900">Set new password</h2>
      <p className="mb-6 text-gray-500">Choose a strong password for your account.</p>

      {serverError ? (
        <div role="alert" className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-600">
          {serverError}{' '}
          <Link href="/forgot-password" className="underline">
            Request new link
          </Link>
        </div>
      ) : null}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div>
          <Input
            label="New password"
            type="password"
            autoComplete="new-password"
            {...register('password', {
              onChange: (e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value),
            })}
            error={errors.password?.message}
          />
          <PasswordStrengthIndicator password={password} />
        </div>
        <Input
          label="Confirm new password"
          type="password"
          autoComplete="new-password"
          {...register('confirmPassword')}
          error={errors.confirmPassword?.message}
        />

        <Button type="submit" isLoading={isSubmitting} size="lg" className="w-full">
          Reset password
        </Button>
      </form>
    </div>
  );
}
