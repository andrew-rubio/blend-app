import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'
import type { HTMLAttributes, ReactNode } from 'react'

export interface CardProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
  variant?: 'default' | 'outlined' | 'elevated'
  padding?: 'none' | 'sm' | 'md' | 'lg'
}

export interface CardHeaderProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
}

export interface CardBodyProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
}

export interface CardFooterProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
}

const variantClasses = {
  default: 'bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800',
  outlined: 'bg-transparent border border-gray-300 dark:border-gray-700',
  elevated: 'bg-white dark:bg-gray-900 shadow-md',
}

const paddingClasses = {
  none: '',
  sm: 'p-3',
  md: 'p-4',
  lg: 'p-6',
}

export function Card({
  children,
  variant = 'default',
  padding = 'md',
  className,
  ...props
}: CardProps) {
  return (
    <div
      className={twMerge(
        clsx('rounded-lg', variantClasses[variant], paddingClasses[padding], className)
      )}
      {...props}
    >
      {children}
    </div>
  )
}

export function CardHeader({ children, className, ...props }: CardHeaderProps) {
  return (
    <div
      className={twMerge(clsx('mb-4 border-b border-gray-200 pb-4 dark:border-gray-800', className))}
      {...props}
    >
      {children}
    </div>
  )
}

export function CardBody({ children, className, ...props }: CardBodyProps) {
  return (
    <div className={twMerge(clsx('flex flex-col gap-4', className))} {...props}>
      {children}
    </div>
  )
}

export function CardFooter({ children, className, ...props }: CardFooterProps) {
  return (
    <div
      className={twMerge(clsx('mt-4 border-t border-gray-200 pt-4 dark:border-gray-800', className))}
      {...props}
    >
      {children}
    </div>
  )
}
