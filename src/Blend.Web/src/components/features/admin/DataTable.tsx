import { clsx } from 'clsx'
import type { ReactNode } from 'react'

export interface DataTableColumn<T> {
  key: string
  header: string
  render: (row: T) => ReactNode
  className?: string
}

export interface DataTableProps<T> {
  columns: DataTableColumn<T>[]
  data: T[]
  keyExtractor: (row: T) => string
  isLoading?: boolean
  emptyMessage?: string
  onRowSelect?: (ids: string[]) => void
  selectedIds?: string[]
}

/**
 * Reusable data table for admin CRUD lists with optional row selection.
 */
export function DataTable<T>({
  columns,
  data,
  keyExtractor,
  isLoading = false,
  emptyMessage = 'No items found.',
  onRowSelect,
  selectedIds = [],
}: DataTableProps<T>) {
  const hasSelection = Boolean(onRowSelect)

  function toggleAll() {
    if (!onRowSelect) return
    if (selectedIds.length === data.length) {
      onRowSelect([])
    } else {
      onRowSelect(data.map(keyExtractor))
    }
  }

  function toggleRow(id: string) {
    if (!onRowSelect) return
    if (selectedIds.includes(id)) {
      onRowSelect(selectedIds.filter((s) => s !== id))
    } else {
      onRowSelect([...selectedIds, id])
    }
  }

  if (isLoading) {
    return (
      <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-800">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 dark:bg-gray-900">
            <tr>
              {hasSelection && <th className="w-10 px-4 py-3" />}
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={clsx(
                    'px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400',
                    col.className
                  )}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {Array.from({ length: 5 }).map((_, i) => (
              <tr key={i} className="border-t border-gray-200 dark:border-gray-800">
                {hasSelection && (
                  <td className="px-4 py-3">
                    <div className="h-4 w-4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
                  </td>
                )}
                {columns.map((col) => (
                  <td key={col.key} className="px-4 py-3">
                    <div className="h-4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
  }

  if (data.length === 0) {
    return (
      <div className="flex items-center justify-center rounded-lg border border-gray-200 py-12 dark:border-gray-800">
        <p className="text-sm text-gray-500 dark:text-gray-400">{emptyMessage}</p>
      </div>
    )
  }

  return (
    <div className="overflow-hidden rounded-lg border border-gray-200 dark:border-gray-800">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 dark:bg-gray-900">
            <tr>
              {hasSelection && (
                <th className="w-10 px-4 py-3">
                  <input
                    type="checkbox"
                    aria-label="Select all"
                    checked={selectedIds.length === data.length && data.length > 0}
                    onChange={toggleAll}
                    className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                  />
                </th>
              )}
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={clsx(
                    'px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400',
                    col.className
                  )}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white dark:divide-gray-800 dark:bg-gray-950">
            {data.map((row) => {
              const id = keyExtractor(row)
              const isSelected = selectedIds.includes(id)
              return (
                <tr
                  key={id}
                  className={clsx(
                    'transition-colors',
                    isSelected && 'bg-primary-50 dark:bg-primary-950'
                  )}
                >
                  {hasSelection && (
                    <td className="px-4 py-3">
                      <input
                        type="checkbox"
                        aria-label={`Select row ${id}`}
                        checked={isSelected}
                        onChange={() => toggleRow(id)}
                        className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                      />
                    </td>
                  )}
                  {columns.map((col) => (
                    <td
                      key={col.key}
                      className={clsx('px-4 py-3 text-gray-900 dark:text-gray-100', col.className)}
                    >
                      {col.render(row)}
                    </td>
                  ))}
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}
