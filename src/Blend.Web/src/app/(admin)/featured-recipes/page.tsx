'use client'

import { useState } from 'react'
import { clsx } from 'clsx'
import {
  useAdminFeaturedRecipes,
  useCreateFeaturedRecipe,
  useUpdateFeaturedRecipe,
  useDeleteFeaturedRecipe,
} from '@/hooks/useAdmin'
import { DataTable } from '@/components/features/admin/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import type { AdminFeaturedRecipe, CreateFeaturedRecipeRequest, UpdateFeaturedRecipeRequest } from '@/types'

interface FeaturedRecipeFormProps {
  initial?: AdminFeaturedRecipe
  onSave: (data: CreateFeaturedRecipeRequest | UpdateFeaturedRecipeRequest) => void
  onCancel: () => void
  isPending: boolean
}

function FeaturedRecipeForm({ initial, onSave, onCancel, isPending }: FeaturedRecipeFormProps) {
  const [recipeId, setRecipeId] = useState(initial?.recipeId ?? '')
  const [title, setTitle] = useState(initial?.title ?? '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [coverImageUrl, setCoverImageUrl] = useState(initial?.coverImageUrl ?? '')
  const [displayOrder, setDisplayOrder] = useState(String(initial?.displayOrder ?? 0))
  const [errors, setErrors] = useState<Record<string, string>>({})

  function validate() {
    const errs: Record<string, string> = {}
    if (!initial && !recipeId.trim()) errs.recipeId = 'Recipe ID is required.'
    if (!title.trim()) errs.title = 'Title is required.'
    if (isNaN(Number(displayOrder))) errs.displayOrder = 'Must be a number.'
    setErrors(errs)
    return Object.keys(errs).length === 0
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!validate()) return
    const payload: CreateFeaturedRecipeRequest | UpdateFeaturedRecipeRequest = {
      ...(initial ? {} : { recipeId: recipeId.trim() }),
      title: title.trim(),
      description: description.trim() || undefined,
      coverImageUrl: coverImageUrl.trim() || undefined,
      displayOrder: Number(displayOrder),
    }
    onSave(payload)
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4 rounded-lg border border-gray-200 p-6 dark:border-gray-800"
      aria-label={initial ? 'Edit featured recipe' : 'Add featured recipe'}
    >
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
        {initial ? 'Edit Featured Recipe' : 'Add Featured Recipe'}
      </h3>
      {!initial && (
        <Input
          label="Recipe ID"
          value={recipeId}
          onChange={(e) => setRecipeId(e.target.value)}
          error={errors.recipeId}
          placeholder="Enter recipe ID or search"
        />
      )}
      <Input
        label="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={errors.title}
        placeholder="Custom display title"
      />
      <div className="flex flex-col gap-1">
        <label className="text-sm font-medium text-gray-700 dark:text-gray-300">Description</label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="block w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm placeholder:text-gray-400 focus:border-primary-500 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
          placeholder="Optional description"
        />
      </div>
      <Input
        label="Cover Image URL"
        value={coverImageUrl}
        onChange={(e) => setCoverImageUrl(e.target.value)}
        placeholder="https://..."
      />
      <Input
        label="Display Order"
        type="number"
        value={displayOrder}
        onChange={(e) => setDisplayOrder(e.target.value)}
        error={errors.displayOrder}
      />
      <div className="flex gap-3">
        <Button type="submit" isLoading={isPending}>
          {initial ? 'Save Changes' : 'Add Recipe'}
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
        <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Remove item?</h2>
        <p className="mb-6 text-sm text-gray-600 dark:text-gray-400">
          <span className="font-medium">&ldquo;{title}&rdquo;</span> will be removed from featured
          content.
        </p>
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={onCancel} disabled={isDeleting} size="sm">
            Cancel
          </Button>
          <Button variant="destructive" onClick={onConfirm} isLoading={isDeleting} size="sm">
            Remove
          </Button>
        </div>
      </div>
    </div>
  )
}

export default function FeaturedRecipesPage() {
  const { data: recipes = [], isLoading } = useAdminFeaturedRecipes()
  const createMutation = useCreateFeaturedRecipe()
  const updateMutation = useUpdateFeaturedRecipe()
  const deleteMutation = useDeleteFeaturedRecipe()

  const [showForm, setShowForm] = useState(false)
  const [editItem, setEditItem] = useState<AdminFeaturedRecipe | null>(null)
  const [deleteItem, setDeleteItem] = useState<AdminFeaturedRecipe | null>(null)

  const sortedRecipes = [...recipes].sort((a, b) => a.displayOrder - b.displayOrder)

  function handleCreate(data: CreateFeaturedRecipeRequest | UpdateFeaturedRecipeRequest) {
    createMutation.mutate(data as CreateFeaturedRecipeRequest, {
      onSuccess: () => setShowForm(false),
    })
  }

  function handleUpdate(data: CreateFeaturedRecipeRequest | UpdateFeaturedRecipeRequest) {
    if (!editItem) return
    updateMutation.mutate(
      { id: editItem.id, data: data as UpdateFeaturedRecipeRequest },
      { onSuccess: () => setEditItem(null) }
    )
  }

  function handleDelete() {
    if (!deleteItem) return
    deleteMutation.mutate(deleteItem.id, { onSuccess: () => setDeleteItem(null) })
  }

  const columns = [
    {
      key: 'order',
      header: 'Order',
      render: (row: AdminFeaturedRecipe) => (
        <span className="font-mono text-xs">{row.displayOrder}</span>
      ),
      className: 'w-16',
    },
    {
      key: 'title',
      header: 'Title',
      render: (row: AdminFeaturedRecipe) => (
        <span className="font-medium">{row.title}</span>
      ),
    },
    {
      key: 'source',
      header: 'Source',
      render: (row: AdminFeaturedRecipe) => (
        <span
          className={clsx(
            'inline-flex rounded-full px-2 py-0.5 text-xs font-medium',
            row.source === 'Community'
              ? 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'
              : 'bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-300'
          )}
        >
          {row.source}
        </span>
      ),
      className: 'w-32',
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row: AdminFeaturedRecipe) => (
        <div className="flex gap-2">
          <Button variant="ghost" size="sm" onClick={() => setEditItem(row)}>
            Edit
          </Button>
          <Button variant="destructive" size="sm" onClick={() => setDeleteItem(row)}>
            Remove
          </Button>
        </div>
      ),
      className: 'w-32',
    },
  ]

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Featured Recipes</h1>
        <Button onClick={() => setShowForm(true)} disabled={showForm}>
          Add Featured Recipe
        </Button>
      </div>

      {showForm && (
        <div className="mb-6">
          <FeaturedRecipeForm
            onSave={handleCreate}
            onCancel={() => setShowForm(false)}
            isPending={createMutation.isPending}
          />
        </div>
      )}

      {editItem && (
        <div className="mb-6">
          <FeaturedRecipeForm
            initial={editItem}
            onSave={handleUpdate}
            onCancel={() => setEditItem(null)}
            isPending={updateMutation.isPending}
          />
        </div>
      )}

      <DataTable
        columns={columns}
        data={sortedRecipes}
        keyExtractor={(row) => row.id}
        isLoading={isLoading}
        emptyMessage="No featured recipes yet. Add one above."
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
