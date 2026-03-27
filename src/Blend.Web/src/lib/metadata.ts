import { cache } from 'react'
import type { Metadata } from 'next'
import type { Recipe, PublicProfile } from '@/types'

const APP_URL = (process.env.NEXT_PUBLIC_APP_URL ?? 'http://localhost:3000').replace(/\/$/, '')
const API_URL = (process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000').replace(/\/$/, '')

/** Absolute URL for the fallback OG image served from the app's public folder. */
export const FALLBACK_OG_IMAGE = `${APP_URL}/og-fallback.png`

// ── Text helpers ──────────────────────────────────────────────────────────────

/** Strip HTML tags from user-generated text to prevent XSS in metadata. */
export function sanitizeText(text: string): string {
  return text.replace(/<[^>]*>/g, '').trim()
}

/** Truncate to `max` characters, appending an ellipsis if needed. */
export function truncate(text: string, max: number): string {
  if (text.length <= max) return text
  return text.slice(0, max - 1).trimEnd() + '\u2026'
}

/** Convert a minute count to an ISO 8601 duration string (e.g. `PT30M`). */
export function minutesToIso8601Duration(minutes: number): string {
  return `PT${minutes}M`
}

/**
 * Safely serialise an object to JSON for use in a `<script>` tag.
 * Escapes `<`, `>`, `/`, `&`, and `'` to prevent script injection.
 */
export function safeJsonStringify(obj: unknown): string {
  return JSON.stringify(obj)
    .replace(/</g, '\\u003c')
    .replace(/>/g, '\\u003e')
    .replace(/\//g, '\\u002f')
    .replace(/&/g, '\\u0026')
    .replace(/'/g, '\\u0027')
}

// ── Share URL ─────────────────────────────────────────────────────────────────

/**
 * Build a shareable recipe URL with UTM tracking parameters.
 * @param recipeId  The recipe ID (will be URI-encoded).
 * @param platform  The sharing medium, e.g. `"web"` or `"mobile"`.
 */
export function buildShareUrl(recipeId: string, platform: string = 'web'): string {
  const url = new URL(`${APP_URL}/recipes/${encodeURIComponent(recipeId)}`)
  url.searchParams.set('utm_source', 'share')
  url.searchParams.set('utm_medium', platform)
  return url.toString()
}

// ── Server-side fetch helpers (React cache for deduplication) ─────────────────

/**
 * Fetch a recipe for server-side metadata generation.
 * Wrapped in `cache()` so that `generateMetadata` and the page component
 * share the same result within a single render.
 */
export const fetchRecipeForMetadata = cache(async (id: string): Promise<Recipe> => {
  const response = await fetch(`${API_URL}/api/v1/recipes/${encodeURIComponent(id)}`, {
    next: { revalidate: 3600 },
  })
  if (!response.ok) {
    const err: { status: number; message: string } = {
      status: response.status,
      message: 'Not found',
    }
    throw err
  }
  return response.json() as Promise<Recipe>
})

/**
 * Fetch a public user profile for server-side metadata generation.
 * Wrapped in `cache()` for deduplication.
 */
export const fetchProfileForMetadata = cache(async (userId: string): Promise<PublicProfile> => {
  const response = await fetch(
    `${API_URL}/api/v1/users/${encodeURIComponent(userId)}/profile`,
    { next: { revalidate: 3600 } }
  )
  if (!response.ok) {
    const err: { status: number; message: string } = {
      status: response.status,
      message: 'Not found',
    }
    throw err
  }
  return response.json() as Promise<PublicProfile>
})

// ── Metadata builders ─────────────────────────────────────────────────────────

/** Build Next.js `Metadata` for a recipe page (OG + Twitter). */
export function buildRecipeMetadata(recipe: Recipe, id: string): Metadata {
  const title = sanitizeText(recipe.title)
  const rawDescription = recipe.description
    ? sanitizeText(recipe.description)
    : `A delicious recipe on Blend.`
  const description = truncate(rawDescription, 200)
  const image = recipe.imageUrl ?? FALLBACK_OG_IMAGE
  const url = `${APP_URL}/recipes/${encodeURIComponent(id)}`

  return {
    title,
    description,
    alternates: { canonical: url },
    openGraph: {
      title,
      description,
      url,
      siteName: 'Blend',
      type: 'article',
      images: [{ url: image }],
    },
    twitter: {
      card: 'summary_large_image',
      title,
      description,
      images: [image],
    },
  }
}

/** Build Next.js `Metadata` for a public user profile page (OG + Twitter). */
export function buildProfileMetadata(profile: PublicProfile, userId: string): Metadata {
  const title = sanitizeText(profile.displayName)
  const recipeWord = profile.recipeCount === 1 ? 'recipe' : 'recipes'
  const description = profile.bio
    ? truncate(sanitizeText(profile.bio), 200)
    : `${title} has ${profile.recipeCount} ${recipeWord} on Blend.`
  const image = profile.avatarUrl ?? FALLBACK_OG_IMAGE
  const url = `${APP_URL}/users/${encodeURIComponent(userId)}`

  return {
    title,
    description,
    alternates: { canonical: url },
    openGraph: {
      title,
      description,
      url,
      siteName: 'Blend',
      type: 'profile',
      images: [{ url: image }],
    },
    twitter: {
      card: 'summary_large_image',
      title,
      description,
      images: [image],
    },
  }
}

// ── JSON-LD structured data ───────────────────────────────────────────────────

/** Build a schema.org `Recipe` JSON-LD object for a recipe page. */
export function buildRecipeJsonLd(recipe: Recipe, id: string): Record<string, unknown> {
  const jsonLd: Record<string, unknown> = {
    '@context': 'https://schema.org',
    '@type': 'Recipe',
    name: sanitizeText(recipe.title),
    url: `${APP_URL}/recipes/${encodeURIComponent(id)}`,
  }

  if (recipe.description) {
    jsonLd.description = sanitizeText(recipe.description)
  }

  if (recipe.imageUrl) {
    jsonLd.image = [recipe.imageUrl]
  }

  if (recipe.author) {
    jsonLd.author = {
      '@type': 'Person',
      name: sanitizeText(recipe.author.name),
    }
  }

  if (recipe.createdAt) {
    jsonLd.datePublished = recipe.createdAt
  }

  if (recipe.prepTimeMinutes != null) {
    jsonLd.prepTime = minutesToIso8601Duration(recipe.prepTimeMinutes)
  }

  if (recipe.cookTimeMinutes != null) {
    jsonLd.cookTime = minutesToIso8601Duration(recipe.cookTimeMinutes)
  }

  if (recipe.readyInMinutes != null) {
    jsonLd.totalTime = minutesToIso8601Duration(recipe.readyInMinutes)
  }

  if (recipe.ingredients.length > 0) {
    jsonLd.recipeIngredient = recipe.ingredients.map((i) =>
      `${i.amount} ${i.unit} ${i.name}`.trim()
    )
  }

  if (recipe.steps.length > 0) {
    jsonLd.recipeInstructions = recipe.steps.map((s) => ({
      '@type': 'HowToStep',
      text: sanitizeText(s.step),
    }))
  }

  if (recipe.cuisines.length > 0) {
    jsonLd.recipeCuisine = recipe.cuisines[0]
  }

  if (recipe.dishTypes.length > 0) {
    jsonLd.recipeCategory = recipe.dishTypes[0]
  }

  return jsonLd
}
