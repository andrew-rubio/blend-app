import { describe, it, expect } from 'vitest'
import sitemap from '@/app/sitemap'

describe('sitemap', () => {
  it('includes static home and explore URLs', () => {
    const entries = sitemap()
    const urls = entries.map((e) => e.url)
    expect(urls).toContain('http://localhost:3000')
    expect(urls).toContain('http://localhost:3000/explore')
  })

  it('returns entries with expected properties', () => {
    const entries = sitemap()
    for (const entry of entries) {
      expect(entry.url).toBeDefined()
      expect(entry.changeFrequency).toBeDefined()
      expect(entry.priority).toBeGreaterThan(0)
    }
  })
})
