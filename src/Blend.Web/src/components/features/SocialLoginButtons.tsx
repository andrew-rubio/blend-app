'use client';

import { Button } from '@/components/ui/Button';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

const PROVIDERS = [
  { id: 'google', label: 'Google', icon: 'G' },
  { id: 'facebook', label: 'Facebook', icon: 'f' },
  { id: 'twitter', label: 'Twitter / X', icon: '𝕏' },
] as const;

export function SocialLoginButtons() {
  const handleSocialLogin = (provider: string) => {
    window.location.href = `${API_URL}/api/v1/auth/login/${provider}`;
  };

  return (
    <div className="space-y-3">
      <div className="relative flex items-center">
        <div className="flex-grow border-t border-gray-200" />
        <span className="mx-4 flex-shrink text-xs text-gray-400">or continue with</span>
        <div className="flex-grow border-t border-gray-200" />
      </div>
      <div className="grid grid-cols-3 gap-3">
        {PROVIDERS.map(({ id, label, icon }) => (
          <Button
            key={id}
            variant="outline"
            onClick={() => handleSocialLogin(id)}
            aria-label={`Continue with ${label}`}
            className="flex flex-col items-center gap-1 py-2"
          >
            <span className="text-lg font-bold">{icon}</span>
            <span className="text-xs">{label}</span>
          </Button>
        ))}
      </div>
    </div>
  );
}
