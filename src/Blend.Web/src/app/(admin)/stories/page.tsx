'use client'

import { useState } from 'react'
import {
  useAdminStories,
  useCreateStory,
  useUpdateStory,
  useDeleteStory,
} from '@/hooks/useAdmin'
import { DataTable } from '@/components/features/admin/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import type { AdminStory, CreateStoryRequest, UpdateStoryRequest } from '@/types'

interface StoryFormProps {
  initial?: AdminStory
  onSave: (data: CreateStoryRequest | UpdateStoryRequest) => void
  onCancel: () => void
  isPending: boolean
}

function StoryForm({ initial, onSave, onCancel, isPending }: StoryFormProps) {
  const [title, setTitle] = useState(initial?.title ?? '')
  const [author, setAuthor] = useState(initial?.author ?? '')
  const [content, setContent] = useState(initial?.content ?? '')
  const [coverImageUrl, setCoverImageUrl] = useState(initial?.coverImageUrl ?? '')
  const [readingTime, setReadingTime] = useState(String(initial?.readingTimeMinutes ?? 5))
  const [preview, setPreview] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  function validate() {
    const errs: Record<string, string> = {}
    if (!title.trim()) errs.title = 'Title is required.'
    if (!author.trim()) errs.author = 'Author is required.'
    if (!content.trim()) errs.content = 'Content is required.'
    if (isNaN(Number(readingTime)) || Number(readingTime) < 1)
      errs.readingTime = 'Reading time must be at least 1 minute.'
    setErrors(errs)
    return Object.keys(errs).length === 0
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return
    onSave({
      title: title.trim(),
      author: author.trim(),
      content: content.trim(),
      coverImageUrl: coverImageUrl.trim() || undefined,
      readingTimeMinutes: Number(readingTime),
      relatedRecipeIds: initial?.relatedRecipeIds ?? [],
    })
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-lg border border-gray-200 p-6 dark:border-gray-800"
      aria-label={initial ? 'Edit story' : 'Create story'}
    >
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
        {initial ? 'Edit Story' : 'Create Story'}
      </h3>
      <Input
        label="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={errors.title}
      />
      <Input
        label="Author"
        value={author}
        onChange={(e) => setAuthor(e.target.value)}
        error={errors.author}
      />
      <Input
        label="Cover Image URL"
        value={coverImageUrl}
        onChange={(e) => setCoverImageUrl(e.target.value)}
        placeholder="https://..."
      />
      <Input
        label="Reading Time (minutes)"
        type="number"
        value={readingTime}
        onChange={(e) => setReadingTime(e.target.value)}
        error={errors.readingTime}
      />
      <div className="flex flex-col gap-1">
        <div className="flex items-center justify-between">
          <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
            Content (Markdown)
          </label>
          <button
            type="button"
            onClick={() => setPreview(!preview)}
            className="text-xs text-primary-600 hover:text-primary-700"
          >
            {preview ? 'Edit' : 'Preview'}
          </button>
        </div>
        {preview ? (
          <div
            className="min-h-32 rounded-md border border-gray-300 bg-white p-3 text-sm whitespace-pre-wrap dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
            aria-label="Content preview"
          >
            {content || <span className="text-gray-400">Nothing to preview.</span>}
          </div>
        ) : (
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={8}
            className="block w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm font-mono placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
            placeholder="Write story content in Markdown..."
            aria-label="Story content"
          />
        )}
        {errors.content && <p className="text-sm text-red-600 dark:text-red-400">{errors.content}</p>}
      </div>
      <div className="flex gap-3">
        <Button type="submit" isLoading={isPending}>
          {initial ? 'Save Changes' : 'Create Story'}
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
        <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Delete story?</h2>
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

export default function StoriesPage() {
  const { data: stories = [], isLoading } = useAdminStories()
  const createMutation = useCreateStory()
  const updateMutation = useUpdateStory()
  const deleteMutation = useDeleteStory()

  const [showForm, setShowForm] = useState(false)
  const [editItem, setEditItem] = useState<AdminStory | null>(null)
  const [deleteItem, setDeleteItem] = useState<AdminStory | null>(null)

  function handleCreate(data: CreateStoryRequest | UpdateStoryRequest) {
    createMutation.mutate(data as CreateStoryRequest, {
      onSuccess: () => setShowForm(false),
    })
  }

  function handleUpdate(data: CreateStoryRequest | UpdateStoryRequest) {
    if (!editItem) return
    updateMutation.mutate(
      { id: editItem.id, data: data as UpdateStoryRequest },
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
      render: (row: AdminStory) => <span className="font-medium">{row.title}</span>,
    },
    {
      key: 'author',
      header: 'Author',
      render: (row: AdminStory) => <span>{row.author}</span>,
      className: 'w-40',
    },
    {
      key: 'readingTime',
      header: 'Reading Time',
      render: (row: AdminStory) => <span>{row.readingTimeMinutes} min</span>,
      className: 'w-32',
    },
    {
      key: 'publishedAt',
      header: 'Published',
      render: (row: AdminStory) =>
        row.publishedAt ? (
          <span>{new Date(row.publishedAt).toLocaleDateString()}</span>
        ) : (
          <span className="text-gray-400">Draft</span>
        ),
      className: 'w-32',
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row: AdminStory) => (
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
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Stories</h1>
        <Button onClick={() => setShowForm(true)} disabled={showForm}>
          Create Story
        </Button>
      </div>

      {showForm && (
        <div className="mb-6">
          <StoryForm
            onSave={handleCreate}
            onCancel={() => setShowForm(false)}
            isPending={createMutation.isPending}
          />
        </div>
      )}

      {editItem && (
        <div className="mb-6">
          <StoryForm
            initial={editItem}
            onSave={handleUpdate}
            onCancel={() => setEditItem(null)}
            isPending={updateMutation.isPending}
          />
        </div>
      )}

      <DataTable
        columns={columns}
        data={stories}
        keyExtractor={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="No stories yet. Create one above."
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
