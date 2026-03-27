import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'
import type { HTMLAttributes } from 'react'

// ── Base skeleton animation class ──────────────────────────────────────────────

const skeletonBase = 'animate-pulse rounded bg-gray-200 dark:bg-gray-700'

// ── Skeleton (generic block) ─────────────────────────────────────────────────

interface SkeletonProps extends HTMLAttributes<HTMLDivElement> {
  width?: string | number
  height?: string | number
}

export function Skeleton({ width, height, className, style, ...props }: SkeletonProps) {
  return (
    <div
      aria-hidden="true"
      className={twMerge(clsx(skeletonBase, className))}
      style={{ width, height, ...style }}
      {...props}
    />
  )
}

// ── SkeletonText ──────────────────────────────────────────────────────────────

interface SkeletonTextProps {
  lines?: number
  className?: string
}

export function SkeletonText({ lines = 3, className }: SkeletonTextProps) {
  return (
    <div aria-hidden="true" className={clsx('flex flex-col gap-2', className)}>
      {Array.from({ length: lines }).map((_, i) => (
        <div
          key={i}
          className={clsx(
            skeletonBase,
            'h-4',
            i === lines - 1 && lines > 1 ? 'w-3/4' : 'w-full'
          )}
        />
      ))}
    </div>
  )
}

// ── SkeletonCard ──────────────────────────────────────────────────────────────

interface SkeletonCardProps {
  className?: string
}

export function SkeletonCard({ className }: SkeletonCardProps) {
  return (
    <div
      aria-hidden="true"
      className={clsx(
        'rounded-lg border border-gray-200 p-4 dark:border-gray-700',
        className
      )}
    >
      <div className={clsx(skeletonBase, 'mb-4 h-40 w-full rounded-md')} />
      <div className={clsx(skeletonBase, 'mb-2 h-4 w-3/4')} />
      <div className={clsx(skeletonBase, 'mb-4 h-3 w-1/2')} />
      <div className="flex gap-2">
        <div className={clsx(skeletonBase, 'h-6 w-16 rounded-full')} />
        <div className={clsx(skeletonBase, 'h-6 w-16 rounded-full')} />
      </div>
    </div>
  )
}

// ── SkeletonSection ───────────────────────────────────────────────────────────

interface SkeletonSectionProps {
  cards?: number
  className?: string
}

export function SkeletonSection({ cards = 3, className }: SkeletonSectionProps) {
  return (
    <div aria-hidden="true" className={clsx('flex flex-col gap-4', className)}>
      <div className={clsx(skeletonBase, 'mb-2 h-6 w-48')} />
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: cards }).map((_, i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
    </div>
  )
}

// ── LoadingSpinner ────────────────────────────────────────────────────────────

interface LoadingSpinnerProps {
  size?: 'sm' | 'md' | 'lg'
  className?: string
}

const spinnerSizes = {
  sm: 'h-4 w-4 border-2',
  md: 'h-6 w-6 border-2',
  lg: 'h-10 w-10 border-4',
}

export function LoadingSpinner({ size = 'md', className }: LoadingSpinnerProps) {
  return (
    <div
      role="status"
      aria-label="Loading"
      className={clsx(
        'animate-spin rounded-full border-gray-200 border-t-primary-600 dark:border-gray-700 dark:border-t-primary-400',
        spinnerSizes[size],
        className
      )}
    />
  )
}
