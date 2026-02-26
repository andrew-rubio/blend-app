'use client'

import { useState } from 'react'
import { clsx } from 'clsx'
import { OverviewTab } from './OverviewTab'
import { IngredientsTab } from './IngredientsTab'
import { DirectionsTab } from './DirectionsTab'
import type { Recipe } from '@/types/recipe'

type TabId = 'overview' | 'ingredients' | 'directions'

const TABS: { id: TabId; label: string }[] = [
  { id: 'overview', label: 'Overview' },
  { id: 'ingredients', label: 'Ingredients' },
  { id: 'directions', label: 'Directions' },
]

interface RecipeTabsProps {
  recipe: Recipe
}

export function RecipeTabs({ recipe }: RecipeTabsProps) {
  const [activeTab, setActiveTab] = useState<TabId>('overview')

  return (
    <div>
      <div
        role="tablist"
        aria-label="Recipe sections"
        className="flex gap-2 overflow-x-auto border-b border-gray-200 pb-0"
      >
        {TABS.map((tab) => (
          <button
            key={tab.id}
            role="tab"
            aria-selected={activeTab === tab.id}
            aria-controls={`tabpanel-${tab.id}`}
            id={`tab-${tab.id}`}
            onClick={() => setActiveTab(tab.id)}
            className={clsx(
              'whitespace-nowrap border-b-2 px-4 py-3 text-sm font-medium transition-colors',
              activeTab === tab.id
                ? 'border-orange-500 text-orange-600'
                : 'border-transparent text-gray-500 hover:text-gray-700',
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="mt-6">
        <div
          role="tabpanel"
          id="tabpanel-overview"
          aria-labelledby="tab-overview"
          hidden={activeTab !== 'overview'}
        >
          <OverviewTab recipe={recipe} />
        </div>
        <div
          role="tabpanel"
          id="tabpanel-ingredients"
          aria-labelledby="tab-ingredients"
          hidden={activeTab !== 'ingredients'}
        >
          <IngredientsTab recipe={recipe} />
        </div>
        <div
          role="tabpanel"
          id="tabpanel-directions"
          aria-labelledby="tab-directions"
          hidden={activeTab !== 'directions'}
        >
          <DirectionsTab steps={recipe.steps} />
        </div>
      </div>
    </div>
  )
}
