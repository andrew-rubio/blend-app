import { type HTMLAttributes } from 'react'
import { clsx } from 'clsx'

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  variant?: 'default' | 'cuisine' | 'diet' | 'intolerance'
}

export function Badge({ variant = 'default', className, children, ...props }: BadgeProps) {
  return (
    <span
      className={clsx(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        {
          'bg-gray-100 text-gray-800': variant === 'default',
          'bg-orange-100 text-orange-800': variant === 'cuisine',
          'bg-green-100 text-green-800': variant === 'diet',
          'bg-blue-100 text-blue-800': variant === 'intolerance',
        },
        className,
      )}
      {...props}
    >
      {children}
    </span>
  )
}
