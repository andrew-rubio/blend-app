'use client'

import { useState, useEffect, useRef } from 'react'

export interface DishNotesProps {
  notes: string
  onSave: (notes: string) => void
  disabled?: boolean
}

const DEBOUNCE_MS = 500

export function DishNotes({ notes, onSave, disabled }: DishNotesProps) {
  const [value, setValue] = useState(notes)
  const [saved, setSaved] = useState(false)
  const isFirstRender = useRef(true)

  useEffect(() => {
    setValue(notes)
  }, [notes])

  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false
      return
    }
    const id = setTimeout(() => {
      onSave(value)
      setSaved(true)
      setTimeout(() => setSaved(false), 1500)
    }, DEBOUNCE_MS)
    return () => clearTimeout(id)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [value])

  return (
    <div className="flex flex-col gap-1" data-testid="dish-notes">
      <div className="flex items-center justify-between">
        <label className="text-xs font-medium text-gray-500 dark:text-gray-400">Notes</label>
        {saved && (
          <span className="text-xs text-green-600 dark:text-green-400" data-testid="dish-notes-saved">
            Saved
          </span>
        )}
      </div>
      <textarea
        value={value}
        onChange={(e) => setValue(e.target.value)}
        disabled={disabled}
        placeholder="Add notes for this dish…"
        rows={3}
        className="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
        data-testid="dish-notes-textarea"
      />
    </div>
  )
}
