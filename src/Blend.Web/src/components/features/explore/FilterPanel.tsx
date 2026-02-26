'use client';

import { SearchFilters } from '@/types/search';

const CUISINES = ['Italian', 'Mexican', 'Japanese', 'Indian', 'Chinese', 'Greek', 'French', 'Thai'];
const DIETS = ['Vegetarian', 'Vegan', 'Gluten Free', 'Dairy Free', 'Ketogenic', 'Paleo'];
const DISH_TYPES = ['main course', 'side dish', 'dessert', 'salad', 'soup', 'breakfast', 'snack'];
const MAX_READY_TIMES = [15, 30, 45, 60, 90];

interface FilterPanelProps {
  isOpen: boolean;
  onClose: () => void;
  filters: SearchFilters;
  onFiltersChange: (filters: SearchFilters) => void;
  onClearAll: () => void;
}

export function FilterPanel({
  isOpen,
  onClose,
  filters,
  onFiltersChange,
  onClearAll,
}: FilterPanelProps) {
  if (!isOpen) return null;

  function toggleArrayItem(arr: string[], item: string): string[] {
    return arr.includes(item) ? arr.filter((i) => i !== item) : [...arr, item];
  }

  return (
    <div className="fixed inset-0 z-50 flex justify-end" role="dialog" aria-label="Filter panel" aria-modal="true">
      <div className="absolute inset-0 bg-black/40" onClick={onClose} aria-hidden="true" />
      <div className="relative w-80 max-w-full h-full bg-white shadow-xl flex flex-col">
        <div className="flex items-center justify-between p-4 border-b">
          <h2 className="text-lg font-semibold">Filters</h2>
          <button
            type="button"
            aria-label="Close filters"
            onClick={onClose}
            className="p-2 rounded-lg hover:bg-gray-100"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-6">
          {/* Cuisines */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Cuisines</h3>
            <div className="flex flex-wrap gap-2">
              {CUISINES.map((c) => (
                <button
                  key={c}
                  type="button"
                  onClick={() =>
                    onFiltersChange({
                      ...filters,
                      cuisines: toggleArrayItem(filters.cuisines, c),
                    })
                  }
                  className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                    filters.cuisines.includes(c)
                      ? 'bg-orange-500 text-white border-orange-500'
                      : 'bg-white text-gray-700 border-gray-200 hover:border-orange-300'
                  }`}
                >
                  {c}
                </button>
              ))}
            </div>
          </div>

          {/* Diets */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Diets</h3>
            <div className="flex flex-wrap gap-2">
              {DIETS.map((d) => (
                <button
                  key={d}
                  type="button"
                  onClick={() =>
                    onFiltersChange({
                      ...filters,
                      diets: toggleArrayItem(filters.diets, d),
                    })
                  }
                  className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                    filters.diets.includes(d)
                      ? 'bg-orange-500 text-white border-orange-500'
                      : 'bg-white text-gray-700 border-gray-200 hover:border-orange-300'
                  }`}
                >
                  {d}
                </button>
              ))}
            </div>
          </div>

          {/* Dish types */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Dish Types</h3>
            <div className="flex flex-wrap gap-2">
              {DISH_TYPES.map((dt) => (
                <button
                  key={dt}
                  type="button"
                  onClick={() =>
                    onFiltersChange({
                      ...filters,
                      dishTypes: toggleArrayItem(filters.dishTypes, dt),
                    })
                  }
                  className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                    filters.dishTypes.includes(dt)
                      ? 'bg-orange-500 text-white border-orange-500'
                      : 'bg-white text-gray-700 border-gray-200 hover:border-orange-300'
                  }`}
                >
                  {dt}
                </button>
              ))}
            </div>
          </div>

          {/* Max ready time */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Max Ready Time</h3>
            <div className="flex flex-wrap gap-2">
              {MAX_READY_TIMES.map((t) => (
                <button
                  key={t}
                  type="button"
                  onClick={() =>
                    onFiltersChange({
                      ...filters,
                      maxReadyTime: filters.maxReadyTime === t ? undefined : t,
                    })
                  }
                  className={`px-3 py-1 rounded-full text-sm border transition-colors ${
                    filters.maxReadyTime === t
                      ? 'bg-orange-500 text-white border-orange-500'
                      : 'bg-white text-gray-700 border-gray-200 hover:border-orange-300'
                  }`}
                >
                  {t} min
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="p-4 border-t">
          <button
            type="button"
            onClick={onClearAll}
            className="w-full py-3 rounded-xl border border-gray-300 text-gray-700 font-medium hover:bg-gray-50 transition-colors"
          >
            Clear all filters
          </button>
        </div>
      </div>
    </div>
  );
}
