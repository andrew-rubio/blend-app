'use client';

const POPULAR_CUISINES = [
  { label: '🇮🇹 Italian', value: 'Italian' },
  { label: '🇲🇽 Mexican', value: 'Mexican' },
  { label: '🇯🇵 Japanese', value: 'Japanese' },
  { label: '🇮🇳 Indian', value: 'Indian' },
  { label: '🇨🇳 Chinese', value: 'Chinese' },
  { label: '🇬🇷 Greek', value: 'Greek' },
];

const POPULAR_DISH_TYPES = [
  { label: '🥗 Salads', value: 'salad' },
  { label: '🍲 Soups', value: 'soup' },
  { label: '🍝 Pasta', value: 'pasta' },
  { label: '🥩 Main Course', value: 'main course' },
  { label: '🍰 Desserts', value: 'dessert' },
];

interface CategoryShortcutsProps {
  onSelectCuisine: (cuisine: string) => void;
  onSelectDishType: (dishType: string) => void;
  activeCuisines: string[];
  activeDishTypes: string[];
}

export function CategoryShortcuts({
  onSelectCuisine,
  onSelectDishType,
  activeCuisines,
  activeDishTypes,
}: CategoryShortcutsProps) {
  return (
    <section aria-label="Category shortcuts">
      <h3 className="text-sm font-medium text-gray-600 mb-2">Cuisines</h3>
      <div className="flex flex-wrap gap-2 mb-4">
        {POPULAR_CUISINES.map((c) => {
          const isActive = activeCuisines.includes(c.value);
          return (
            <button
              key={c.value}
              type="button"
              onClick={() => onSelectCuisine(c.value)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium border transition-colors ${
                isActive
                  ? 'bg-orange-500 text-white border-orange-500'
                  : 'bg-white text-gray-700 border-gray-200 hover:border-orange-300'
              }`}
            >
              {c.label}
            </button>
          );
        })}
      </div>
      <h3 className="text-sm font-medium text-gray-600 mb-2">Dish Types</h3>
      <div className="flex flex-wrap gap-2">
        {POPULAR_DISH_TYPES.map((d) => {
          const isActive = activeDishTypes.includes(d.value);
          return (
            <button
              key={d.value}
              type="button"
              onClick={() => onSelectDishType(d.value)}
              className={`px-3 py-1.5 rounded-full text-sm font-medium border transition-colors ${
                isActive
                  ? 'bg-orange-500 text-white border-orange-500'
                  : 'bg-white text-gray-700 border-gray-200 hover:border-orange-300'
              }`}
            >
              {d.label}
            </button>
          );
        })}
      </div>
    </section>
  );
}
