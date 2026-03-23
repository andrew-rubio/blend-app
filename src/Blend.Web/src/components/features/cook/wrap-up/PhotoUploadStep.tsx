'use client'

interface PhotoUploadStepProps {
  photos: string[]
  primaryPhotoIndex: number
  onAdd: (url: string) => void
  onRemove: (index: number) => void
  onSetPrimary: (index: number) => void
  onNext: () => void
  onSkip: () => void
  isUploading?: boolean
}

/**
 * Step 3: Photo Upload (COOK-36 through COOK-39).
 * Upload up to 5 photos with preview, reordering, deletion and primary marking.
 *
 * Note: In a full implementation this integrates with the media upload pipeline
 * (SAS token → direct upload → processing). For now it accepts URLs directly,
 * consistent with the media upload task (task 009) integration point.
 */
export function PhotoUploadStep({
  photos,
  primaryPhotoIndex,
  onAdd,
  onRemove,
  onSetPrimary,
  onNext,
  onSkip,
  isUploading = false,
}: PhotoUploadStepProps) {
  const maxPhotos = 5

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const files = e.target.files
    if (!files) return
    // In a real implementation this would upload via SAS token pipeline.
    // For now we create a local object URL as a placeholder.
    Array.from(files)
      .slice(0, maxPhotos - photos.length)
      .forEach((file) => {
        const url = URL.createObjectURL(file)
        onAdd(url)
      })
    e.target.value = ''
  }

  return (
    <div data-testid="photo-upload-step">
      <h2 className="mb-1 text-xl font-semibold text-gray-900 dark:text-white">Upload Photos</h2>
      <p className="mb-6 text-sm text-gray-500 dark:text-gray-400">
        Add up to {maxPhotos} photos of your finished dish. The first photo will be used as the
        cover image.
      </p>

      {/* Photo grid */}
      {photos.length > 0 && (
        <div
          className="mb-4 grid grid-cols-2 gap-3 sm:grid-cols-3"
          data-testid="photo-grid"
          aria-label="Uploaded photos"
        >
          {photos.map((url, index) => (
            <div
              key={url}
              className={`relative rounded-lg overflow-hidden border-2 ${
                index === primaryPhotoIndex
                  ? 'border-primary-500'
                  : 'border-gray-200 dark:border-gray-700'
              }`}
              data-testid={`photo-${index}`}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={url}
                alt={`Dish photo ${index + 1}`}
                className="h-32 w-full object-cover"
              />
              <div className="absolute inset-x-0 bottom-0 flex items-center justify-between bg-black/50 px-2 py-1">
                <button
                  type="button"
                  onClick={() => onSetPrimary(index)}
                  className="text-xs text-white hover:text-yellow-300"
                  aria-label={`Set photo ${index + 1} as primary`}
                  aria-pressed={index === primaryPhotoIndex}
                  data-testid={`set-primary-${index}`}
                >
                  {index === primaryPhotoIndex ? '⭐ Primary' : 'Set primary'}
                </button>
                <button
                  type="button"
                  onClick={() => onRemove(index)}
                  className="text-xs text-red-300 hover:text-red-100"
                  aria-label={`Remove photo ${index + 1}`}
                  data-testid={`remove-photo-${index}`}
                >
                  ✕
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Upload button */}
      {photos.length < maxPhotos && (
        <label
          className="mb-6 flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-gray-300 p-8 transition-colors hover:border-primary-400 dark:border-gray-600 dark:hover:border-primary-500"
          aria-label="Upload photos"
          data-testid="photo-upload-area"
        >
          <span className="mb-1 text-2xl" aria-hidden="true">
            📷
          </span>
          <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
            {isUploading ? 'Uploading…' : 'Click to add photos'}
          </span>
          <span className="text-xs text-gray-400 dark:text-gray-500">
            {photos.length}/{maxPhotos} photos added
          </span>
          <input
            type="file"
            accept="image/*"
            multiple
            className="sr-only"
            onChange={handleFileChange}
            disabled={isUploading}
            data-testid="photo-file-input"
          />
        </label>
      )}

      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onSkip}
          className="rounded-md border border-gray-300 px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-800"
          aria-label="Skip photo upload"
          data-testid="photos-skip-button"
        >
          Skip
        </button>
        <button
          type="button"
          onClick={onNext}
          className="rounded-md bg-primary-600 px-5 py-2 text-sm font-medium text-white hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500"
          aria-label="Continue to publish step"
          data-testid="photos-next-button"
        >
          Continue
        </button>
      </div>
    </div>
  )
}
