'use client'

import React, {
  createContext,
  useCallback,
  useContext,
  useReducer,
  useRef,
} from 'react'
import type { ReactNode } from 'react'
import { clsx } from 'clsx'

// ── Types ─────────────────────────────────────────────────────────────────────

export type ToastVariant = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: string
  message: string
  variant: ToastVariant
  duration?: number
  action?: {
    label: string
    onClick: () => void
  }
}

interface ToastState {
  toasts: Toast[]
}

type ToastAction =
  | { type: 'ADD'; toast: Toast }
  | { type: 'REMOVE'; id: string }

// ── Reducer ───────────────────────────────────────────────────────────────────

function toastReducer(state: ToastState, action: ToastAction): ToastState {
  switch (action.type) {
    case 'ADD':
      return { toasts: [...state.toasts, action.toast] }
    case 'REMOVE':
      return { toasts: state.toasts.filter((t) => t.id !== action.id) }
    default:
      return state
  }
}

// ── Context ───────────────────────────────────────────────────────────────────

interface ToastContextValue {
  toasts: Toast[]
  addToast: (
    message: string,
    options?: {
      variant?: ToastVariant
      duration?: number
      action?: Toast['action']
    }
  ) => void
  removeToast: (id: string) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

// ── Provider ──────────────────────────────────────────────────────────────────

interface ToastProviderProps {
  children: ReactNode
}

export function ToastProvider({ children }: ToastProviderProps) {
  const [state, dispatch] = useReducer(toastReducer, { toasts: [] })
  const timerRefs = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map())

  const removeToast = useCallback((id: string) => {
    dispatch({ type: 'REMOVE', id })
    const timer = timerRefs.current.get(id)
    if (timer !== undefined) {
      clearTimeout(timer)
      timerRefs.current.delete(id)
    }
  }, [])

  const addToast = useCallback(
    (
      message: string,
      options?: {
        variant?: ToastVariant
        duration?: number
        action?: Toast['action']
      }
    ) => {
      const id = `toast-${Date.now()}-${Math.random().toString(36).slice(2)}`
      const duration = options?.duration ?? 5000
      const toast: Toast = {
        id,
        message,
        variant: options?.variant ?? 'info',
        duration,
        action: options?.action,
      }
      dispatch({ type: 'ADD', toast })
      if (duration > 0) {
        const timer = setTimeout(() => removeToast(id), duration)
        timerRefs.current.set(id, timer)
      }
    },
    [removeToast]
  )

  return (
    <ToastContext.Provider value={{ toasts: state.toasts, addToast, removeToast }}>
      {children}
      <ToastContainer />
    </ToastContext.Provider>
  )
}

// ── Hook ──────────────────────────────────────────────────────────────────────

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastContext)
  if (!ctx) {
    throw new Error('useToast must be used within a ToastProvider')
  }
  return ctx
}

// ── Components ────────────────────────────────────────────────────────────────

const variantClasses: Record<ToastVariant, string> = {
  success: 'bg-green-50 border-green-400 text-green-800 dark:bg-green-900/30 dark:border-green-600 dark:text-green-300',
  error:   'bg-red-50 border-red-400 text-red-800 dark:bg-red-900/30 dark:border-red-600 dark:text-red-300',
  warning: 'bg-yellow-50 border-yellow-400 text-yellow-800 dark:bg-yellow-900/30 dark:border-yellow-600 dark:text-yellow-300',
  info:    'bg-blue-50 border-blue-400 text-blue-800 dark:bg-blue-900/30 dark:border-blue-600 dark:text-blue-300',
}

const variantIcons: Record<ToastVariant, string> = {
  success: '✓',
  error:   '✕',
  warning: '⚠',
  info:    'ℹ',
}

function ToastItem({ toast, onDismiss }: { toast: Toast; onDismiss: (id: string) => void }) {
  return (
    <div
      role="alert"
      aria-live="polite"
      className={clsx(
        'flex items-start gap-3 rounded-lg border px-4 py-3 shadow-md',
        variantClasses[toast.variant]
      )}
    >
      <span className="shrink-0 text-sm font-bold" aria-hidden>
        {variantIcons[toast.variant]}
      </span>
      <p className="flex-1 text-sm">{toast.message}</p>
      {toast.action && (
        <button
          onClick={toast.action.onClick}
          className="shrink-0 text-sm font-medium underline hover:no-underline"
        >
          {toast.action.label}
        </button>
      )}
      <button
        onClick={() => onDismiss(toast.id)}
        aria-label="Dismiss notification"
        className="shrink-0 text-sm opacity-60 hover:opacity-100"
      >
        ✕
      </button>
    </div>
  )
}

function ToastContainer() {
  const { toasts, removeToast } = useContext(ToastContext)!
  if (toasts.length === 0) {
    return null
  }

  return (
    <div
      aria-label="Notifications"
      className="fixed bottom-4 right-4 z-50 flex w-80 max-w-full flex-col gap-2"
    >
      {toasts.map((toast) => (
        <ToastItem key={toast.id} toast={toast} onDismiss={removeToast} />
      ))}
    </div>
  )
}
