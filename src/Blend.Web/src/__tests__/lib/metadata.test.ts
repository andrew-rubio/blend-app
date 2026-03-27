import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { Recipe, PublicProfile } from '@/types'
import {
  sanitizeText,
  truncate,
  minutesToIso8601Duration,
  safeJsonStringify,
  buildShareUrl,
  buildRecipeMetadata,
  buildProfileMetadata,
  buildRecipeJsonLd,
  FALLBACK_OG_IMAGE,
} from '@/lib/metadata'

// ── Fixtures ──────────────────────────────────────────────────────────────────

const mockRecipe: Recipe = {
  id: 'r1',
  title: 'Spaghetti Carbonara',
  description: 'Classic Italian pasta dish with eggs, cheese, and pancetta.',
  imageUrl: 'https://cdn.example.com/carbonara.jpg',
  cuisines: ['Italian'],
  dishTypes: ['main course'],
  diets: ['gluten free'],
  intolerances: [],
  servings: 4,
  readyInMinutes: 30,
  prepTimeMinutes: 10,
  cookTimeMinutes: 20,
  difficulty: 'Medium',
  ingredients: [
    { id: 'i1', name: 'pasta', amount: 200, unit: 'g' },
    { id: 'i2', name: 'pancetta', amount: 100, unit: 'g' },
  ],
  steps: [
    { number: 1, step: 'Boil the pasta until al dente.' },
    { number: 2, step: 'Fry the pancetta until crispy.' },
  ],
  dataSource: 'Community',
  author: { id: 'u1', name: 'Chef Mario', avatarUrl: undefined },
  likeCount: 42,
  isLiked: false,
  createdAt: '2024-01-15T10:00:00Z',
}

const mockProfile: PublicProfile = {
  id: 'u1',
  displayName: 'Chef Mario',
  avatarUrl: 'https://cdn.example.com/avatar.jpg',
  bio: 'Passionate about Italian cuisine.',
  joinDate: '2023-01-01T00:00:00Z',
  recipeCount: 12,
}

// ── sanitizeText ──────────────────────────────────────────────────────────────

describe('sanitizeText', () => {
  it('strips HTML tags', () => {
    expect(sanitizeText('<b>Hello</b> <em>World</em>')).toBe('Hello World')
  })

  it('trims leading and trailing whitespace', () => {
    expect(sanitizeText('  hello  ')).toBe('hello')
  })

  it('returns plain text unchanged', () => {
    expect(sanitizeText('No tags here')).toBe('No tags here')
  })

  it('removes script tags to prevent XSS', () => {
    // The tag markers are stripped; the inner text remains as safe plain text
    expect(sanitizeText('<script>alert("xss")</script>title')).toBe('alert("xss")title')
  })
})

// ── truncate ──────────────────────────────────────────────────────────────────

describe('truncate', () => {
  it('returns the string unchanged when within max', () => {
    expect(truncate('short', 10)).toBe('short')
  })

  it('truncates to max characters and appends ellipsis', () => {
    const result = truncate('Hello World', 8)
    expect(result.length).toBeLessThanOrEqual(8)
    expect(result).toMatch(/\u2026$/)
  })

  it('returns unchanged string when exactly max length', () => {
    expect(truncate('12345', 5)).toBe('12345')
  })
})

// ── minutesToIso8601Duration ──────────────────────────────────────────────────

describe('minutesToIso8601Duration', () => {
  it('converts 30 minutes to PT30M', () => {
    expect(minutesToIso8601Duration(30)).toBe('PT30M')
  })

  it('converts 0 minutes to PT0M', () => {
    expect(minutesToIso8601Duration(0)).toBe('PT0M')
  })

  it('converts 90 minutes to PT90M', () => {
    expect(minutesToIso8601Duration(90)).toBe('PT90M')
  })
})

// ── safeJsonStringify ─────────────────────────────────────────────────────────

describe('safeJsonStringify', () => {
  it('serialises a plain object', () => {
    const result = safeJsonStringify({ name: 'Blend' })
    expect(result).toContain('"name"')
    expect(result).toContain('"Blend"')
  })

  it('escapes < and > to prevent script injection', () => {
    const result = safeJsonStringify({ title: '<script>alert(1)</script>' })
    expect(result).not.toContain('<script>')
    expect(result).toContain('\\u003c')
    expect(result).toContain('\\u003e')
  })

  it('escapes / to prevent </script> injection', () => {
    const result = safeJsonStringify({ path: '</script>' })
    expect(result).not.toContain('</script>')
    expect(result).toContain('\\u002f')
  })

  it('escapes & and single quotes', () => {
    const result = safeJsonStringify({ val: "a&b'c" })
    expect(result).not.toContain('&')
    expect(result).not.toContain("'")
    expect(result).toContain('\\u0026')
    expect(result).toContain('\\u0027')
  })
})

// ── buildShareUrl ─────────────────────────────────────────────────────────────

describe('buildShareUrl', () => {
  it('includes utm_source=share', () => {
    const url = buildShareUrl('r1')
    expect(url).toContain('utm_source=share')
  })

  it('defaults to utm_medium=web', () => {
    const url = buildShareUrl('r1')
    expect(url).toContain('utm_medium=web')
  })

  it('uses the supplied platform as utm_medium', () => {
    const url = buildShareUrl('r1', 'mobile')
    expect(url).toContain('utm_medium=mobile')
  })

  it('includes the recipe ID in the path', () => {
    const url = buildShareUrl('r1')
    expect(url).toContain('/recipes/r1')
  })

  it('URL-encodes special characters in recipe ID', () => {
    const url = buildShareUrl('hello world')
    expect(url).toContain('hello%20world')
  })
})

// ── buildRecipeMetadata ───────────────────────────────────────────────────────

describe('buildRecipeMetadata', () => {
  it('sets the page title to the recipe name', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    expect(metadata.title).toBe('Spaghetti Carbonara')
  })

  it('sets og:title to the recipe name', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    expect((metadata.openGraph as { title?: string })?.title).toBe('Spaghetti Carbonara')
  })

  it('sets og:type to article', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    expect((metadata.openGraph as { type?: string })?.type).toBe('article')
  })

  it('sets og:site_name to Blend', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    expect((metadata.openGraph as { siteName?: string })?.siteName).toBe('Blend')
  })

  it('includes the recipe image in og:image', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    const images = (metadata.openGraph as { images?: { url: string }[] })?.images
    expect(images?.[0]?.url).toBe(mockRecipe.imageUrl)
  })

  it('falls back to FALLBACK_OG_IMAGE when no imageUrl', () => {
    const recipe = { ...mockRecipe, imageUrl: undefined }
    const metadata = buildRecipeMetadata(recipe, 'r1')
    const images = (metadata.openGraph as { images?: { url: string }[] })?.images
    expect(images?.[0]?.url).toBe(FALLBACK_OG_IMAGE)
  })

  it('truncates description to 200 chars', () => {
    const longDesc = 'A'.repeat(300)
    const recipe = { ...mockRecipe, description: longDesc }
    const metadata = buildRecipeMetadata(recipe, 'r1')
    expect((metadata.description as string).length).toBeLessThanOrEqual(200)
  })

  it('generates a fallback description when recipe has no description', () => {
    const recipe = { ...mockRecipe, description: undefined }
    const metadata = buildRecipeMetadata(recipe, 'r1')
    expect(metadata.description).toBeTruthy()
  })

  it('sets twitter:card to summary_large_image', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    expect((metadata.twitter as { card?: string })?.card).toBe('summary_large_image')
  })

  it('sets twitter:title matching og:title', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    expect((metadata.twitter as { title?: string })?.title).toBe('Spaghetti Carbonara')
  })

  it('sets canonical URL', () => {
    const metadata = buildRecipeMetadata(mockRecipe, 'r1')
    const canonical = (metadata.alternates as { canonical?: string })?.canonical
    expect(canonical).toContain('/recipes/r1')
  })

  it('sanitises HTML in recipe title', () => {
    const recipe = { ...mockRecipe, title: '<b>Bold Title</b>' }
    const metadata = buildRecipeMetadata(recipe, 'r1')
    expect(metadata.title).toBe('Bold Title')
    expect(metadata.title).not.toContain('<b>')
  })
})

// ── buildProfileMetadata ──────────────────────────────────────────────────────

describe('buildProfileMetadata', () => {
  it('sets the page title to the user display name', () => {
    const metadata = buildProfileMetadata(mockProfile, 'u1')
    expect(metadata.title).toBe('Chef Mario')
  })

  it('uses bio as description when provided', () => {
    const metadata = buildProfileMetadata(mockProfile, 'u1')
    expect(metadata.description).toBe('Passionate about Italian cuisine.')
  })

  it('generates a fallback description from recipe count when no bio', () => {
    const profile = { ...mockProfile, bio: undefined }
    const metadata = buildProfileMetadata(profile, 'u1')
    expect(metadata.description).toContain('12')
    expect(metadata.description).toContain('recipes')
  })

  it('uses singular "recipe" for recipeCount of 1', () => {
    const profile = { ...mockProfile, bio: undefined, recipeCount: 1 }
    const metadata = buildProfileMetadata(profile, 'u1')
    expect(metadata.description).toContain('1 recipe')
    expect(metadata.description).not.toContain('recipes')
  })

  it('includes the avatar as og:image', () => {
    const metadata = buildProfileMetadata(mockProfile, 'u1')
    const images = (metadata.openGraph as { images?: { url: string }[] })?.images
    expect(images?.[0]?.url).toBe(mockProfile.avatarUrl)
  })

  it('falls back to FALLBACK_OG_IMAGE when no avatar', () => {
    const profile = { ...mockProfile, avatarUrl: undefined }
    const metadata = buildProfileMetadata(profile, 'u1')
    const images = (metadata.openGraph as { images?: { url: string }[] })?.images
    expect(images?.[0]?.url).toBe(FALLBACK_OG_IMAGE)
  })

  it('sets twitter:card to summary_large_image', () => {
    const metadata = buildProfileMetadata(mockProfile, 'u1')
    expect((metadata.twitter as { card?: string })?.card).toBe('summary_large_image')
  })

  it('sets og:type to profile', () => {
    const metadata = buildProfileMetadata(mockProfile, 'u1')
    expect((metadata.openGraph as { type?: string })?.type).toBe('profile')
  })
})

// ── buildRecipeJsonLd ─────────────────────────────────────────────────────────

describe('buildRecipeJsonLd', () => {
  it('sets @context to https://schema.org', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd['@context']).toBe('https://schema.org')
  })

  it('sets @type to Recipe', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd['@type']).toBe('Recipe')
  })

  it('includes the recipe name', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.name).toBe('Spaghetti Carbonara')
  })

  it('includes the description', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.description).toBe(mockRecipe.description)
  })

  it('includes the image URL in an array', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.image).toEqual([mockRecipe.imageUrl])
  })

  it('omits image when recipe has no imageUrl', () => {
    const recipe = { ...mockRecipe, imageUrl: undefined }
    const jsonLd = buildRecipeJsonLd(recipe, 'r1')
    expect(jsonLd.image).toBeUndefined()
  })

  it('includes author with @type Person', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.author).toMatchObject({ '@type': 'Person', name: 'Chef Mario' })
  })

  it('omits author when recipe has no author', () => {
    const recipe = { ...mockRecipe, author: undefined }
    const jsonLd = buildRecipeJsonLd(recipe, 'r1')
    expect(jsonLd.author).toBeUndefined()
  })

  it('includes datePublished from createdAt', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.datePublished).toBe('2024-01-15T10:00:00Z')
  })

  it('generates ISO 8601 prepTime', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.prepTime).toBe('PT10M')
  })

  it('generates ISO 8601 cookTime', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.cookTime).toBe('PT20M')
  })

  it('generates ISO 8601 totalTime from readyInMinutes', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.totalTime).toBe('PT30M')
  })

  it('omits prepTime/cookTime/totalTime when not provided', () => {
    const recipe = {
      ...mockRecipe,
      prepTimeMinutes: undefined,
      cookTimeMinutes: undefined,
      readyInMinutes: undefined,
    }
    const jsonLd = buildRecipeJsonLd(recipe, 'r1')
    expect(jsonLd.prepTime).toBeUndefined()
    expect(jsonLd.cookTime).toBeUndefined()
    expect(jsonLd.totalTime).toBeUndefined()
  })

  it('includes recipeIngredient as array of strings', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    const ingredients = jsonLd.recipeIngredient as string[]
    expect(Array.isArray(ingredients)).toBe(true)
    expect(ingredients[0]).toContain('pasta')
    expect(ingredients[1]).toContain('pancetta')
  })

  it('includes recipeInstructions as HowToStep array', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    const instructions = jsonLd.recipeInstructions as { '@type': string; text: string }[]
    expect(Array.isArray(instructions)).toBe(true)
    expect(instructions[0]).toMatchObject({ '@type': 'HowToStep', text: 'Boil the pasta until al dente.' })
  })

  it('sets recipeCuisine from first cuisine', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.recipeCuisine).toBe('Italian')
  })

  it('sets recipeCategory from first dishType', () => {
    const jsonLd = buildRecipeJsonLd(mockRecipe, 'r1')
    expect(jsonLd.recipeCategory).toBe('main course')
  })

  it('sanitises HTML in step text', () => {
    const recipe = {
      ...mockRecipe,
      steps: [{ number: 1, step: '<script>xss()</script>Boil pasta.' }],
    }
    const jsonLd = buildRecipeJsonLd(recipe, 'r1')
    const instructions = jsonLd.recipeInstructions as { text: string }[]
    expect(instructions[0].text).not.toContain('<script>')
    expect(instructions[0].text).toContain('Boil pasta.')
  })
})
