'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { SocialLoginButtons } from '@/components/features/SocialLoginButtons';
import { PasswordStrengthIndicator } from '@/components/features/PasswordStrengthIndicator';
import { authApi } from '@/lib/apiClient';
import { useAuthStore } from '@/stores/authStore';
import type { AuthResponse } from '@/types/auth';

const schema = z
  .object({
    name: z.string().min(2, 'Display name must be at least 2 characters'),
    email: z.string().email('Enter a valid email address'),
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/[A-Z]/, 'Password must contain an uppercase letter')
      .regex(/[a-z]/, 'Password must contain a lowercase letter')
      .regex(/[0-9]/, 'Password must contain a number'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type FormData = z.infer<typeof schema>;

export default function RegisterPage() {
  const router = useRouter();
  const { setUser } = useAuthStore();
  const [serverError, setServerError] = useState<string | null>(null);
  const [password, setPassword] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    setServerError(null);
    try {
      const res = await authApi.register(data.name, data.email, data.password);
      const { user, accessToken } = res.data as AuthResponse;
      setUser(user, accessToken);
      router.push('/preferences');
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Registration failed. Please try again.';
      setServerError(message);
    }
  };

  return (
    <div className="rounded-2xl bg-white p-8 shadow-sm">
      <h2 className="mb-6 text-2xl font-bold text-gray-900">Create account</h2>

      {serverError ? (
        <div role="alert" className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-600">
          {serverError}
        </div>
      ) : null}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <Input
          label="Display name"
          type="text"
          autoComplete="name"
          {...register('name')}
          error={errors.name?.message}
        />
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
        />
        <div>
          <Input
            label="Password"
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
          label="Confirm password"
          type="password"
          autoComplete="new-password"
          {...register('confirmPassword')}
          error={errors.confirmPassword?.message}
        />

        <Button type="submit" isLoading={isSubmitting} size="lg" className="w-full">
          Create account
        </Button>

        <p className="text-center text-xs text-gray-400">
          <button
            type="button"
            onClick={() => router.push('/')}
            className="text-gray-500 underline hover:text-gray-700"
          >
            Browse as guest instead
          </button>
        </p>
      </form>

      <div className="mt-6">
        <SocialLoginButtons />
      </div>

      <p className="mt-6 text-center text-sm text-gray-500">
        Already have an account?{' '}
        <Link href="/login" className="font-medium text-green-600 hover:underline">
          Log in
        </Link>
      </p>
    </div>
  );
}
