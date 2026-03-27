'use client'

import React, { Component } from 'react'
import type { ReactNode, ErrorInfo } from 'react'
import { Button } from './Button'

interface ErrorBoundaryState {
  hasError: boolean
  error: Error | null
}

interface ErrorBoundaryProps {
  children: ReactNode
  fallback?: ReactNode
  onError?: (error: Error, errorInfo: ErrorInfo) => void
  level?: 'root' | 'page' | 'section'
}

/**
 * Generic React Error Boundary component.
 * Supports root, page, and section error levels per PLAT-04 through PLAT-06.
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props)
    this.state = { hasError: false, error: null }
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    this.props.onError?.(error, errorInfo)
    if (process.env.NODE_ENV !== 'test') {
      console.error('ErrorBoundary caught an error:', error, errorInfo)
    }
  }

  handleReset = (): void => {
    this.setState({ hasError: false, error: null })
  }

  render(): ReactNode {
    if (!this.state.hasError) {
      return this.props.children
    }

    if (this.props.fallback) {
      return this.props.fallback
    }

    const { level = 'root' } = this.props

    if (level === 'section') {
      return <SectionErrorFallback onRetry={this.handleReset} />
    }

    if (level === 'page') {
      return <PageErrorFallback onRetry={this.handleReset} />
    }

    return <RootErrorFallback onRetry={this.handleReset} />
  }
}

interface FallbackProps {
  onRetry: () => void
}

function RootErrorFallback({ onRetry }: FallbackProps) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4">
      <div className="mx-auto max-w-md text-center">
        <h1 className="mb-4 text-3xl font-bold text-gray-900 dark:text-white">
          Something went wrong
        </h1>
        <p className="mb-8 text-gray-600 dark:text-gray-400">
          An unexpected error occurred. Please try again.
        </p>
        <Button variant="primary" onClick={onRetry}>
          Try again
        </Button>
      </div>
    </div>
  )
}

function PageErrorFallback({ onRetry }: FallbackProps) {
  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center p-4">
      <div className="mx-auto max-w-md text-center">
        <h2 className="mb-4 text-2xl font-bold text-gray-900 dark:text-white">
          This page could not be loaded
        </h2>
        <p className="mb-6 text-gray-600 dark:text-gray-400">
          There was a problem loading this page. You can try again or navigate to another page.
        </p>
        <div className="flex justify-center gap-3">
          <Button variant="primary" onClick={onRetry}>
            Try again
          </Button>
          <Button variant="outline" onClick={() => (window.location.href = '/')}>
            Go home
          </Button>
        </div>
      </div>
    </div>
  )
}

function SectionErrorFallback({ onRetry }: FallbackProps) {
  return (
    <div className="flex min-h-[200px] flex-col items-center justify-center rounded-lg border border-gray-200 p-6 dark:border-gray-700">
      <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
        Could not load this section
      </p>
      <Button variant="outline" size="sm" onClick={onRetry}>
        Retry
      </Button>
    </div>
  )
}

/**
 * Convenience wrappers for each error boundary level.
 */
export function RootErrorBoundary({ children }: { children: ReactNode }) {
  return <ErrorBoundary level="root">{children}</ErrorBoundary>
}

export function PageErrorBoundary({ children }: { children: ReactNode }) {
  return <ErrorBoundary level="page">{children}</ErrorBoundary>
}

export function SectionErrorBoundary({ children }: { children: ReactNode }) {
  return <ErrorBoundary level="section">{children}</ErrorBoundary>
}
