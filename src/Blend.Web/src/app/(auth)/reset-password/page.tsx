import { Suspense } from 'react';
import { ResetPasswordForm } from './ResetPasswordForm';

export default function ResetPasswordPage() {
  return (
    <Suspense
      fallback={
        <div className="rounded-2xl bg-white p-8 shadow-sm text-center">
          <p className="text-gray-500">Loading...</p>
        </div>
      }
    >
      <ResetPasswordForm />
    </Suspense>
  );
}
