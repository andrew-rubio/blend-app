export type PasswordStrength = 'too-short' | 'weak' | 'fair' | 'strong' | 'very-strong'

export interface PasswordRequirements {
  minLength: boolean
  hasUppercase: boolean
  hasLowercase: boolean
  hasNumber: boolean
  hasSpecial: boolean
}

export function checkPasswordRequirements(password: string): PasswordRequirements {
  return {
    minLength: password.length >= 8,
    hasUppercase: /[A-Z]/.test(password),
    hasLowercase: /[a-z]/.test(password),
    hasNumber: /[0-9]/.test(password),
    hasSpecial: /[^A-Za-z0-9]/.test(password),
  }
}

export function getPasswordStrength(password: string): PasswordStrength {
  if (password.length < 8) return 'too-short'

  const reqs = checkPasswordRequirements(password)
  const metCount = [
    reqs.hasUppercase,
    reqs.hasLowercase,
    reqs.hasNumber,
    reqs.hasSpecial,
  ].filter(Boolean).length

  if (metCount <= 1) return 'weak'
  if (metCount === 2) return 'fair'
  if (metCount === 3) return 'strong'
  if (password.length >= 12 && metCount === 4) return 'very-strong'
  return 'strong'
}

export function isPasswordValid(password: string): boolean {
  const reqs = checkPasswordRequirements(password)
  return reqs.minLength && reqs.hasUppercase && reqs.hasLowercase && reqs.hasNumber && reqs.hasSpecial
}

export const strengthLabels: Record<PasswordStrength, string> = {
  'too-short': 'Too short',
  weak: 'Weak',
  fair: 'Fair',
  strong: 'Strong',
  'very-strong': 'Very strong',
}

export const strengthColors: Record<PasswordStrength, string> = {
  'too-short': 'bg-red-500',
  weak: 'bg-red-400',
  fair: 'bg-yellow-400',
  strong: 'bg-green-400',
  'very-strong': 'bg-green-600',
}

export const strengthWidths: Record<PasswordStrength, string> = {
  'too-short': 'w-1/5',
  weak: 'w-2/5',
  fair: 'w-3/5',
  strong: 'w-4/5',
  'very-strong': 'w-full',
}
