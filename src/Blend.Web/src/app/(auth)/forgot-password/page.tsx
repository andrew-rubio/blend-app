'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Link from 'next/link';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { authApi } from '@/lib/apiClient';

const schema = z.object({
  email: z.string().email('Enter a valid email address'),
});

type FormData = z.infer<typeof schema>;

export default function ForgotPasswordPage() {
  const [submitted, setSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    try {
      await authApi.forgotPassword(data.email);
    } catch {
      // Swallow error — always show success to prevent enumeration
    } finally {
      setSubmitted(true);
    }
  };

  if (submitted) {
    return (
      <div className="rounded-2xl bg-white p-8 shadow-sm text-center">
        <div className="mb-4 text-5xl">📧</div>
        <h2 className="mb-2 text-2xl font-bold text-gray-900">Check your email</h2>
        <p className="mb-6 text-gray-500">
          If an account exists for that email, we&apos;ve sent a password reset link.
        </p>
        <Link href="/login">
          <Button variant="outline" size="lg" className="w-full">
            Back to log in
          </Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="rounded-2xl bg-white p-8 shadow-sm">
      <h2 className="mb-2 text-2xl font-bold text-gray-900">Reset password</h2>
      <p className="mb-6 text-gray-500">
        Enter your email address and we&apos;ll send you a reset link.
      </p>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          {...register('email')}
          error={errors.email?.message}
        />

        <Button type="submit" isLoading={isSubmitting} size="lg" className="w-full">
          Send reset link
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-gray-500">
        <Link href="/login" className="font-medium text-green-600 hover:underline">
          Back to log in
        </Link>
      </p>
    </div>
  );
}
