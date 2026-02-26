'use client';

import { Button } from '@/components/ui/Button';
import { useRouter } from 'next/navigation';

interface GuestPromptModalProps {
  isOpen: boolean;
  onClose: () => void;
  message?: string;
}

export function GuestPromptModal({
  isOpen,
  onClose,
  message = 'Create an account to access this feature',
}: GuestPromptModalProps) {
  const router = useRouter();

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      role="dialog"
      aria-modal="true"
      aria-label="Login required"
      onClick={onClose}
    >
      <div
        className="w-full max-w-sm rounded-2xl bg-white p-8 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 text-center text-4xl">🔒</div>
        <h2 className="mb-2 text-center text-xl font-bold text-gray-900">Login required</h2>
        <p className="mb-6 text-center text-gray-500">{message}</p>
        <div className="flex flex-col gap-3">
          <Button
            onClick={() => {
              onClose();
              router.push('/register');
            }}
            size="lg"
            className="w-full"
          >
            Create an account
          </Button>
          <Button
            onClick={() => {
              onClose();
              router.push('/login');
            }}
            variant="outline"
            size="lg"
            className="w-full"
          >
            Log in
          </Button>
          <Button onClick={onClose} variant="ghost" size="lg" className="w-full text-gray-500">
            Maybe later
          </Button>
        </div>
      </div>
    </div>
  );
}
