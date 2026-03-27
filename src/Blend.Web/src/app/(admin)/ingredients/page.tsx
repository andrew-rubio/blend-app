'use client'

import { useState } from 'react'
import { clsx } from 'clsx'
import {
  useAdminSubmissions,
  useApproveSubmission,
  useRejectSubmission,
  useBatchApproveSubmissions,
  useBatchRejectSubmissions,
} from '@/hooks/useAdmin'
import { DataTable } from '@/components/features/admin/DataTable'
import { Button } from '@/components/ui/Button'
import type { AdminIngredientSubmission, IngredientSubmissionStatus } from '@/types'

const STATUS_TABS: { label: string; value: IngredientSubmissionStatus | undefined }[] = [
  { label: 'Pending', value: 'Pending' },
  { label: 'Approved', value: 'Approved' },
  { label: 'Rejected', value: 'Rejected' },
]

interface RejectDialogProps {
  submissionName: string
  onConfirm: (reason: string) => void
  onCancel: () => void
  isPending: boolean
}

function RejectDialog({ submissionName, onConfirm, onCancel, isPending }: RejectDialogProps) {
  const [reason, setReason] = useState('')

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Reject submission"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    >
      <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-xl dark:bg-gray-900">
        <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">
          Reject submission?
        </h2>
        <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
          Rejecting <span className="font-medium">&ldquo;{submissionName}&rdquo;</span>.
        </p>
        <div className="mb-4 flex flex-col gap-1">
          <label
            htmlFor="reject-reason"
            className="text-sm font-medium text-gray-700 dark:text-gray-300"
          >
            Reason (optional)
          </label>
          <textarea
            id="reject-reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            className="block w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
            placeholder="Optional rejection reason..."
          />
        </div>
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={onCancel} disabled={isPending} size="sm">
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={() => onConfirm(reason)}
            isLoading={isPending}
            size="sm"
          >
            Reject
          </Button>
        </div>
      </div>
    </div>
  )
}

export default function IngredientsPage() {
  const [activeStatus, setActiveStatus] = useState<IngredientSubmissionStatus>('Pending')
  const [page, setPage] = useState(1)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [rejectTarget, setRejectTarget] = useState<AdminIngredientSubmission | null>(null)

  const { data, isLoading } = useAdminSubmissions(activeStatus, page)
  const approveMutation = useApproveSubmission()
  const rejectMutation = useRejectSubmission()
  const batchApproveMutation = useBatchApproveSubmissions()
  const batchRejectMutation = useBatchRejectSubmissions()

  const submissions = data?.submissions ?? []
  const totalPages = data?.totalPages ?? 1

  function handleTabChange(status: IngredientSubmissionStatus) {
    setActiveStatus(status)
    setPage(1)
    setSelectedIds([])
  }

  function handleApprove(id: string) {
    approveMutation.mutate({ id })
  }

  function handleRejectConfirm(reason: string) {
    if (!rejectTarget) return
    rejectMutation.mutate(
      { id: rejectTarget.id, data: { reason: reason || undefined } },
      { onSuccess: () => setRejectTarget(null) }
    )
  }

  function handleBatchApprove() {
    batchApproveMutation.mutate({ ids: selectedIds }, { onSuccess: () => setSelectedIds([]) })
  }

  function handleBatchReject() {
    batchRejectMutation.mutate({ ids: selectedIds }, { onSuccess: () => setSelectedIds([]) })
  }

  const columns = [
    {
      key: 'name',
      header: 'Ingredient',
      render: (row: AdminIngredientSubmission) => (
        <span className="font-medium">{row.name}</span>
      ),
    },
    {
      key: 'category',
      header: 'Category',
      render: (row: AdminIngredientSubmission) => <span>{row.category}</span>,
      className: 'w-32',
    },
    {
      key: 'submittedBy',
      header: 'Submitted By',
      render: (row: AdminIngredientSubmission) => <span>{row.submittedByName}</span>,
      className: 'w-40',
    },
    {
      key: 'submittedAt',
      header: 'Date',
      render: (row: AdminIngredientSubmission) => (
        <span>{new Date(row.submittedAt).toLocaleDateString()}</span>
      ),
      className: 'w-28',
    },
    ...(activeStatus === 'Pending'
      ? [
          {
            key: 'actions',
            header: 'Actions',
            render: (row: AdminIngredientSubmission) => (
              <div className="flex gap-2">
                <Button
                  variant="primary"
                  size="sm"
                  onClick={() => handleApprove(row.id)}
                  isLoading={approveMutation.isPending && approveMutation.variables?.id === row.id}
                >
                  Approve
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => setRejectTarget(row)}
                >
                  Reject
                </Button>
              </div>
            ),
            className: 'w-40',
          },
        ]
      : [
          {
            key: 'status',
            header: 'Status',
            render: (row: AdminIngredientSubmission) => (
              <span
                className={clsx(
                  'inline-flex rounded-full px-2 py-0.5 text-xs font-medium',
                  row.status === 'Approved'
                    ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
                    : 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300'
                )}
              >
                {row.status}
              </span>
            ),
            className: 'w-24',
          },
        ]),
  ]

  const isPendingTab = activeStatus === 'Pending'

  return (
    <div>
      <h1 className="mb-6 text-3xl font-bold text-gray-900 dark:text-white">
        Ingredient Submissions
      </h1>

      {/* Status filter tabs */}
      <div
        className="mb-6 flex border-b border-gray-200 dark:border-gray-800"
        role="tablist"
        aria-label="Submission status filter"
      >
        {STATUS_TABS.map((tab) => (
          <button
            key={tab.label}
            role="tab"
            aria-selected={activeStatus === tab.value}
            onClick={() => handleTabChange(tab.value as IngredientSubmissionStatus)}
            className={clsx(
              'px-4 py-2 text-sm font-medium transition-colors',
              activeStatus === tab.value
                ? 'border-b-2 border-primary-600 text-primary-600 dark:text-primary-400'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Batch actions (pending tab only) */}
      {isPendingTab && selectedIds.length > 0 && (
        <div className="mb-4 flex items-center gap-3 rounded-lg bg-gray-50 p-3 dark:bg-gray-900">
          <span className="text-sm text-gray-600 dark:text-gray-400">
            {selectedIds.length} selected
          </span>
          <Button
            variant="primary"
            size="sm"
            onClick={handleBatchApprove}
            isLoading={batchApproveMutation.isPending}
          >
            Approve Selected
          </Button>
          <Button
            variant="destructive"
            size="sm"
            onClick={handleBatchReject}
            isLoading={batchRejectMutation.isPending}
          >
            Reject Selected
          </Button>
          <button
            onClick={() => setSelectedIds([])}
            className="ml-auto text-sm text-gray-500 hover:text-gray-700"
          >
            Clear selection
          </button>
        </div>
      )}

      <DataTable
        columns={columns}
        data={submissions}
        keyExtractor={(row) => row.id}
        isLoading={isLoading}
        emptyMessage={`No ${activeStatus.toLowerCase()} submissions.`}
        onRowSelect={isPendingTab ? setSelectedIds : undefined}
        selectedIds={selectedIds}
      />

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
          >
            Previous
          </Button>
          <span className="text-sm text-gray-500 dark:text-gray-400">
            Page {page} of {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages}
          >
            Next
          </Button>
        </div>
      )}

      {rejectTarget && (
        <RejectDialog
          submissionName={rejectTarget.name}
          onConfirm={handleRejectConfirm}
          onCancel={() => setRejectTarget(null)}
          isPending={rejectMutation.isPending}
        />
      )}
    </div>
  )
}
