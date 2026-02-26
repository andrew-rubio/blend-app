# Task 009: Media Upload Pipeline

> **GitHub Issue:** [#8](https://github.com/andrew-rubio/blend-app/issues/8)

## Description

Implement the media upload system per ADR 0007: Azure Blob Storage for file storage, SAS-token-based direct browser uploads, and image processing (resize, optimise, thumbnail generation). This service supports profile photo uploads (REQ-42), recipe step photos/videos (REQ-25), and post-cook wrap-up photos (REQ-23).

## Dependencies

- **001-task-backend-scaffolding** — requires the API project
- **005-task-database-setup** — requires Cosmos DB for storing media metadata

## Technical Requirements

### Azure Blob Storage configuration

- Configure Azure Blob Storage client via dependency injection
- Register the Blob Storage resource in the Aspire AppHost (use Azurite emulator for local dev)
- Create the blob container structure (per ADR 0007):
  - `blend-media/recipes/{recipeId}/{version}/` — recipe images
  - `blend-media/profiles/{userId}/` — profile photos
  - `blend-media/content/{contentId}/` — admin content images

### SAS token generation endpoint

- `POST /api/v1/media/upload-url` — authenticated endpoint that returns a time-limited SAS URL for the specific blob path
- Request must include: content type, intended use (profile, recipe, content), associated entity ID
- Validate:
  - User is authenticated
  - Content-Type is in the allowed list: JPEG, PNG, WebP for images; MP4 for video (PLAT-27)
  - Intended file size is within limits (communicated via request body) (PLAT-28)
- Return: SAS URL with write permissions, blob path, expiry time
- SAS token lifetime: 5-15 minutes (configurable)
- SAS token is scoped to the specific blob path (user cannot write to other users' paths)

### Image processing service

- Implement an `IImageProcessingService` that processes uploaded images:
  - Validate the image (verify it's a real image, not just a renamed file)
  - Resize to configured breakpoints:
    - Hero: 1200px width (recipe hero, content banners)
    - Card: 600px width (recipe cards in lists)
    - Thumbnail: 300px width (small previews)
    - Avatar: 200px × 200px (profile photos)
  - Convert to WebP format for optimal web delivery
  - Write optimised variants alongside the original
- For **local development**: trigger processing synchronously after upload notification
- For **production**: designed to be triggered via Azure Function BlobTrigger (the Function itself is deployed via infrastructure task, but the processing logic lives in a shared library)

### Upload notification endpoint

- `POST /api/v1/media/upload-complete` — called by the frontend after successful direct upload to Blob Storage
- Triggers image processing (in dev) or records the upload for async processing (in prod)
- Updates the associated entity metadata in Cosmos DB (e.g., sets `profilePhotoUrl` on the user, adds photo URL to recipe)

### Media retrieval

- Uploaded and processed media is served via Azure CDN (production) or direct Blob Storage URL (development)
- The API returns CDN-based URLs in all entity responses
- Configure appropriate `Cache-Control` headers on blobs (`max-age=31536000` for versioned paths)

### Error handling

- Invalid file type → return error with accepted formats list
- File too large → return error with maximum size
- Upload failed → mark the upload as failed in metadata, notify the user
- Image processing failed → log error, keep original, return a flag indicating processing is pending

## Acceptance Criteria

- [ ] `POST /api/v1/media/upload-url` returns a valid SAS URL for authenticated users
- [ ] SAS URLs are scoped to the correct blob path and expire after the configured time
- [ ] Invalid content types are rejected with a clear error
- [ ] Oversized files are rejected with a clear error
- [ ] Direct browser upload to Blob Storage via SAS URL succeeds
- [ ] Image processing generates WebP variants at configured breakpoints
- [ ] Profile photo uploads result in a 200px avatar variant
- [ ] Recipe photo uploads result in hero, card, and thumbnail variants
- [ ] Upload completion updates the associated entity in Cosmos DB
- [ ] Media URLs in API responses point to the CDN (production) or Blob Storage (development)
- [ ] Failed processing is logged and the original image remains accessible

## Testing Requirements

- Unit tests for SAS URL generation (correct path, correct permissions, correct expiry)
- Unit tests for file type and size validation
- Unit tests for image processing (resize dimensions, format conversion)
- Integration tests for the upload flow: generate SAS → upload → notify complete → verify variants exist
- Integration tests against Azurite (local Blob Storage emulator)
- Unit tests for error handling (invalid type, oversize, processing failure)
- Minimum 85% code coverage for all media upload and processing code
