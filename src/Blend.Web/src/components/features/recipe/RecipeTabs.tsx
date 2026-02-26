'use client'

import { useState } from 'react'
import type { Recipe } from '@/types/recipe'
import { OverviewTab } from './OverviewTab'
import { IngredientsTab } from './IngredientsTab'
import { DirectionsTab } from './DirectionsTab'

type Tab = 'overview' | 'ingredients' | 'directions'

const TABS: { id: Tab; label: string }[] = [
  { id: 'overview', label: 'Overview' },
  { id: 'ingredients', label: 'Ingredients' },
  { id: 'directions', label: 'Directions' },
]

interface Props { recipe: Recipe }

export function RecipeTabs({ recipe }: Props) {
  const [activeTab, setActiveTab] = useState<Tab>('overview')

  return (
    <div className="mt-6">
      <div role="tablist" className="flex gap-2 overflow-x-auto border-b border-gray-200 pb-px">
        {TABS.map(({ id, label }) => (
          <button
            key={id}
            role="tab"
            id={`tab-${id}`}
            aria-selected={activeTab === id}
            aria-controls={`tabpanel-${id}`}
            onClick={() => setActiveTab(id)}
            className={`whitespace-nowrap rounded-t-lg px-4 py-2 text-sm font-medium transition-colors ${
              activeTab === id
                ? 'border-b-2 border-orange-500 text-orange-600'
                : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            {label}
          </button>
        ))}
      </div>
      <div
        role="tabpanel"
        id={`tabpanel-${activeTab}`}
        aria-labelledby={`tab-${activeTab}`}
        className="mt-4"
      >
        {activeTab === 'overview' && <OverviewTab recipe={recipe} />}
        {activeTab === 'ingredients' && <IngredientsTab recipe={recipe} />}
        {activeTab === 'directions' && <DirectionsTab recipe={recipe} />}
      </div>
    </div>
  )
}
