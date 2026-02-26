'use client';

import { getPasswordStrength, PASSWORD_REQUIREMENTS } from '@/lib/passwordStrength';

interface PasswordStrengthIndicatorProps {
  password: string;
}

export function PasswordStrengthIndicator({ password }: PasswordStrengthIndicatorProps) {
  if (!password) return null;

  const strength = getPasswordStrength(password);

  return (
    <div className="mt-2 space-y-2">
      <div className="flex gap-1">
        {[0, 1, 2, 3].map((i) => (
          <div
            key={i}
            className={`h-1.5 flex-1 rounded-full transition-colors ${
              i < strength.score ? strength.color : 'bg-gray-200'
            }`}
          />
        ))}
      </div>
      <p className="text-xs text-gray-500">
        Password strength: <span className="font-medium">{strength.label}</span>
      </p>
      <ul className="space-y-1">
        {PASSWORD_REQUIREMENTS.map((req) => (
          <li
            key={req.label}
            className={`flex items-center gap-1 text-xs ${
              req.test(password) ? 'text-green-600' : 'text-gray-400'
            }`}
          >
            <span>{req.test(password) ? '✓' : '○'}</span>
            {req.label}
          </li>
        ))}
      </ul>
    </div>
  );
}
