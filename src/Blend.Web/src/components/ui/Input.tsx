import { forwardRef } from 'react'
import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'
import type { InputHTMLAttributes, ReactNode } from 'react'

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  hint?: string
  leftAddon?: ReactNode
  rightAddon?: ReactNode
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, hint, leftAddon, rightAddon, className, id, ...props }, ref) => {
    const inputId = id ?? label?.toLowerCase().replace(/\s+/g, '-')

    return (
      <div className="flex flex-col gap-1">
        {label && (
          <label
            htmlFor={inputId}
            className="text-sm font-medium text-gray-700 dark:text-gray-300"
          >
            {label}
          </label>
        )}
        <div className="relative flex items-center">
          {leftAddon && (
            <div className="absolute left-3 flex items-center text-gray-400">{leftAddon}</div>
          )}
          <input
            ref={ref}
            id={inputId}
            className={twMerge(
              clsx(
                'block w-full rounded-md border bg-white px-3 py-2 text-sm',
                'placeholder:text-gray-400',
                'focus:outline-none focus:ring-2 focus:ring-offset-0',
                'disabled:cursor-not-allowed disabled:opacity-50',
                'dark:bg-gray-900 dark:text-gray-100',
                error
                  ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
                  : 'border-gray-300 focus:border-primary-500 focus:ring-primary-500 dark:border-gray-600',
                leftAddon && 'pl-10',
                rightAddon && 'pr-10',
                className
              )
            )}
            {...props}
          />
          {rightAddon && (
            <div className="absolute right-3 flex items-center text-gray-400">{rightAddon}</div>
          )}
        </div>
        {error && <p className="text-sm text-red-600 dark:text-red-400">{error}</p>}
        {hint && !error && <p className="text-sm text-gray-500 dark:text-gray-400">{hint}</p>}
      </div>
    )
  }
)

Input.displayName = 'Input'
