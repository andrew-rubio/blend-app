'use client'

import { useState } from 'react'
import {
  useAdminVideos,
  useCreateVideo,
  useUpdateVideo,
  useDeleteVideo,
} from '@/hooks/useAdmin'
import { DataTable } from '@/components/features/admin/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import type { AdminVideo, CreateVideoRequest, UpdateVideoRequest } from '@/types'

interface VideoFormProps {
  initial?: AdminVideo
  onSave: (data: CreateVideoRequest | UpdateVideoRequest) => void
  onCancel: () => void
  isPending: boolean
}

function VideoForm({ initial, onSave, onCancel, isPending }: VideoFormProps) {
  const [title, setTitle] = useState(initial?.title ?? '')
  const [creator, setCreator] = useState(initial?.creator ?? '')
  const [embedUrl, setEmbedUrl] = useState(initial?.embedUrl ?? '')
  const [thumbnailUrl, setThumbnailUrl] = useState(initial?.thumbnailUrl ?? '')
  const [duration, setDuration] = useState(String(initial?.durationSeconds ?? ''))
  const [errors, setErrors] = useState<Record<string, string>>({})

  function validate() {
    const errs: Record<string, string> = {}
    if (!title.trim()) errs.title = 'Title is required.'
    if (!creator.trim()) errs.creator = 'Creator is required.'
    if (!embedUrl.trim()) errs.embedUrl = 'Embed URL is required.'
    if (duration && isNaN(Number(duration))) errs.duration = 'Duration must be a number.'
    setErrors(errs)
    return Object.keys(errs).length === 0
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return
    onSave({
      title: title.trim(),
      creator: creator.trim(),
      embedUrl: embedUrl.trim(),
      thumbnailUrl: thumbnailUrl.trim() || undefined,
      durationSeconds: duration ? Number(duration) : undefined,
    })
  }

  const isYouTube = embedUrl.includes('youtube.com') || embedUrl.includes('youtu.be')

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-lg border border-gray-200 p-6 dark:border-gray-800"
      aria-label={initial ? 'Edit video' : 'Add video'}
    >
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
        {initial ? 'Edit Video' : 'Add Video'}
      </h3>
      <Input
        label="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={errors.title}
      />
      <Input
        label="Creator"
        value={creator}
        onChange={(e) => setCreator(e.target.value)}
        error={errors.creator}
      />
      <Input
        label="Embed URL"
        value={embedUrl}
        onChange={(e) => setEmbedUrl(e.target.value)}
        error={errors.embedUrl}
        placeholder="https://www.youtube.com/embed/..."
      />
      {isYouTube && embedUrl && (
        <div className="aspect-video w-full max-w-sm overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700">
          <iframe
            src={embedUrl}
            title="Video preview"
            className="h-full w-full"
            allowFullScreen
          />
        </div>
      )}
      <Input
        label="Thumbnail URL"
        value={thumbnailUrl}
        onChange={(e) => setThumbnailUrl(e.target.value)}
        placeholder="https://..."
      />
      <Input
        label="Duration (seconds)"
        type="number"
        value={duration}
        onChange={(e) => setDuration(e.target.value)}
        error={errors.duration}
        placeholder="e.g. 360"
      />
      <div className="flex gap-3">
        <Button type="submit" isLoading={isPending}>
          {initial ? 'Save Changes' : 'Add Video'}
        </Button>
        <Button type="button" variant="outline" onClick={onCancel} disabled={isPending}>
          Cancel
        </Button>
      </div>
    </form>
  )
}

interface DeleteDialogProps {
  title: string
  onConfirm: () => void
  onCancel: () => void
  isDeleting: boolean
}

function DeleteDialog({ title, onConfirm, onCancel, isDeleting }: DeleteDialogProps) {
  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Confirm deletion"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    >
      <div className="w-full max-w-sm rounded-xl bg-white p-6 shadow-xl dark:bg-gray-900">
        <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Delete video?</h2>
        <p className="mb-6 text-sm text-gray-600 dark:text-gray-400">
          <span className="font-medium">&ldquo;{title}&rdquo;</span> will be permanently deleted.
        </p>
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={onCancel} disabled={isDeleting} size="sm">
            Cancel
          </Button>
          <Button variant="destructive" onClick={onConfirm} isLoading={isDeleting} size="sm">
            Delete
          </Button>
        </div>
      </div>
    </div>
  )
}

function formatDuration(seconds?: number): string {
  if (!seconds) return '—'
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return `${m}:${String(s).padStart(2, '0')}`
}

export default function VideosPage() {
  const { data: videos = [], isLoading } = useAdminVideos()
  const createMutation = useCreateVideo()
  const updateMutation = useUpdateVideo()
  const deleteMutation = useDeleteVideo()

  const [showForm, setShowForm] = useState(false)
  const [editItem, setEditItem] = useState<AdminVideo | null>(null)
  const [deleteItem, setDeleteItem] = useState<AdminVideo | null>(null)

  function handleCreate(data: CreateVideoRequest | UpdateVideoRequest) {
    createMutation.mutate(data as CreateVideoRequest, {
      onSuccess: () => setShowForm(false),
    })
  }

  function handleUpdate(data: CreateVideoRequest | UpdateVideoRequest) {
    if (!editItem) return
    updateMutation.mutate(
      { id: editItem.id, data: data as UpdateVideoRequest },
      { onSuccess: () => setEditItem(null) }
    )
  }

  function handleDelete() {
    if (!deleteItem) return
    deleteMutation.mutate(deleteItem.id, { onSuccess: () => setDeleteItem(null) })
  }

  const columns = [
    {
      key: 'title',
      header: 'Title',
      render: (row: AdminVideo) => <span className="font-medium">{row.title}</span>,
    },
    {
      key: 'creator',
      header: 'Creator',
      render: (row: AdminVideo) => <span>{row.creator}</span>,
      className: 'w-40',
    },
    {
      key: 'duration',
      header: 'Duration',
      render: (row: AdminVideo) => <span>{formatDuration(row.durationSeconds)}</span>,
      className: 'w-24',
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row: AdminVideo) => (
        <div className="flex gap-2">
          <Button variant="ghost" size="sm" onClick={() => setEditItem(row)}>
            Edit
          </Button>
          <Button variant="destructive" size="sm" onClick={() => setDeleteItem(row)}>
            Delete
          </Button>
        </div>
      ),
      className: 'w-32',
    },
  ]

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Videos</h1>
        <Button onClick={() => setShowForm(true)} disabled={showForm}>
          Add Video
        </Button>
      </div>

      {showForm && (
        <div className="mb-6">
          <VideoForm
            onSave={handleCreate}
            onCancel={() => setShowForm(false)}
            isPending={createMutation.isPending}
          />
        </div>
      )}

      {editItem && (
        <div className="mb-6">
          <VideoForm
            initial={editItem}
            onSave={handleUpdate}
            onCancel={() => setEditItem(null)}
            isPending={updateMutation.isPending}
          />
        </div>
      )}

      <DataTable
        columns={columns}
        data={videos}
        keyExtractor={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="No videos yet. Add one above."
      />

      {deleteItem && (
        <DeleteDialog
          title={deleteItem.title}
          onConfirm={handleDelete}
          onCancel={() => setDeleteItem(null)}
          isDeleting={deleteMutation.isPending}
        />
      )}
    </div>
  )
}
