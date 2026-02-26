import type { PasswordStrength } from '@/types/auth';

export function getPasswordStrength(password: string): PasswordStrength {
  let score = 0;

  if (password.length >= 8) score++;
  if (password.length >= 12) score++;
  if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;

  const capped = Math.min(score, 4) as 0 | 1 | 2 | 3 | 4;

  const map: Record<
    0 | 1 | 2 | 3 | 4,
    { label: PasswordStrength['label']; color: string }
  > = {
    0: { label: 'Very weak', color: 'bg-red-500' },
    1: { label: 'Weak', color: 'bg-orange-500' },
    2: { label: 'Fair', color: 'bg-yellow-500' },
    3: { label: 'Strong', color: 'bg-blue-500' },
    4: { label: 'Very strong', color: 'bg-green-500' },
  };

  return { score: capped, ...map[capped] };
}

export const PASSWORD_REQUIREMENTS = [
  { label: 'At least 8 characters', test: (p: string) => p.length >= 8 },
  { label: 'Uppercase letter', test: (p: string) => /[A-Z]/.test(p) },
  { label: 'Lowercase letter', test: (p: string) => /[a-z]/.test(p) },
  { label: 'Number', test: (p: string) => /[0-9]/.test(p) },
  {
    label: 'Special character',
    test: (p: string) => /[^A-Za-z0-9]/.test(p),
  },
];
