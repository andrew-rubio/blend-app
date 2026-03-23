'use client'

import { TrendingSection } from './TrendingSection'
import { RecommendedSection } from './RecommendedSection'
import { CategoryShortcuts } from './CategoryShortcuts'

export interface ExploreLandingProps {
  /** Called when the user selects a category shortcut (to update the search query / filter). */
  onCategorySelect?: (value: string) => void
}

/**
 * Explore landing page — shown when no search query is active (EXPL-01).
 * Renders trending recipes, recommended recipes, and category shortcuts.
 */
export function ExploreLanding({ onCategorySelect }: ExploreLandingProps) {
  return (
    <div className="flex flex-col gap-8">
      <CategoryShortcuts onSelect={onCategorySelect} />
      <TrendingSection />
      <RecommendedSection />
    </div>
  )
}
