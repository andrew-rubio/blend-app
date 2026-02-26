# Task 009 – Media Upload Pipeline

## Overview

Implement a complete media upload pipeline for the Blend API using Azure Blob Storage and SixLabors.ImageSharp.

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/v1/media/upload-url` | JWT Bearer | Generate a write-only SAS URL for direct client upload |
| POST | `/api/v1/media/upload-complete` | JWT Bearer | Confirm upload, trigger image processing, return variant URLs |

## Flow

```
Client → POST /upload-url → SAS Token
Client → PUT {SAS URL}    → Azure Blob (direct upload)
Client → POST /upload-complete → triggers WebP variants
```

## Supported Content Types

| Type | Max Size |
|------|----------|
| image/jpeg | 50 MB |
| image/png | 50 MB |
| image/webp | 50 MB |
| video/mp4 | 500 MB |

## Image Variants (WebP)

| Variant | Dimensions | Entity Types |
|---------|-----------|--------------|
| hero | 1200px wide (proportional) | recipe, content |
| card | 600px wide (proportional) | recipe, content |
| thumbnail | 300px wide (proportional) | recipe, content |
| avatar | 200×200px (crop) | profile |

## Blob Paths

- `recipes/{recipeId}/{fileName}`
- `profiles/{userId}/{fileName}`
- `content/{contentId}/{fileName}`

## Configuration

```json
{
  "BlobStorage": {
    "ConnectionString": "<connection-string>",
    "ContainerName": "blend-media",
    "CdnBaseUrl": null,
    "SasExpiryMinutes": 15,
    "MaxImageSizeBytes": 52428800,
    "MaxVideoSizeBytes": 524288000
  },
  "Media": {
    "HeroWidth": 1200,
    "CardWidth": 600,
    "ThumbnailWidth": 300,
    "AvatarSize": 200,
    "WebPQuality": 85
  }
}
```

## Implementation Files

- `src/Blend.Api/Configuration/BlobStorageOptions.cs`
- `src/Blend.Api/Configuration/MediaOptions.cs`
- `src/Blend.Api/Models/MediaUploadRequest.cs`
- `src/Blend.Api/Models/MediaUploadResponse.cs`
- `src/Blend.Api/Models/UploadCompleteRequest.cs`
- `src/Blend.Api/Models/UploadCompleteResponse.cs`
- `src/Blend.Api/Services/IBlobStorageService.cs`
- `src/Blend.Api/Services/BlobStorageService.cs`
- `src/Blend.Api/Services/IImageProcessingService.cs`
- `src/Blend.Api/Services/ImageProcessingService.cs`
- `src/Blend.Api/Services/IMediaService.cs`
- `src/Blend.Api/Services/MediaService.cs`
- `src/Blend.Api/Controllers/MediaController.cs`
- `src/Blend.Api/Program.cs`

## Test Coverage

- `BlobStorageServiceTests` – CDN URL construction, default option values
- `ImageProcessingServiceTests` – magic byte validation per format, invalid content type rejection
- `MediaServiceTests` – blob path routing, SAS generation, size limits, image/video branching, entity-type to MediaType mapping
- `MediaControllerTests` – 200 responses, 400 on invalid inputs, correlation ID header propagation

## NuGet Dependencies

| Package | Version |
|---------|---------|
| Azure.Storage.Blobs | 12.27.0 |
| SixLabors.ImageSharp | 3.1.12 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.2 |
| Moq (tests) | 4.20.72 |
| Microsoft.AspNetCore.Mvc.Testing (tests) | 9.0.2 |
