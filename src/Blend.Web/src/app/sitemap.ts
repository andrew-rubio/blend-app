import type { MetadataRoute } from 'next'

export const dynamic = 'force-static'

const APP_URL = (process.env.NEXT_PUBLIC_APP_URL ?? 'http://localhost:3000').replace(/\/$/, '')

export default function sitemap(): MetadataRoute.Sitemap {
  return [
    {
      url: APP_URL,
      changeFrequency: 'daily',
      priority: 1.0,
    },
    {
      url: `${APP_URL}/explore`,
      changeFrequency: 'daily',
      priority: 0.9,
    },
  ]
}
