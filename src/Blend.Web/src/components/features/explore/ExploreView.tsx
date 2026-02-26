'use client';

import { SearchResult, SearchFilters } from '@/types/search';
import { TrendingSection } from './TrendingSection';
import { RecommendedSection } from './RecommendedSection';
import { CategoryShortcuts } from './CategoryShortcuts';

// Mock data for explore landing (trending + recommended)
const MOCK_TRENDING: SearchResult[] = [
  { id: '1', title: 'Spaghetti Carbonara', image: '', cuisines: ['Italian'], readyInMinutes: 30, likes: 1240, dataSource: 'community' },
  { id: '2', title: 'Chicken Tikka Masala', image: '', cuisines: ['Indian'], readyInMinutes: 45, likes: 980, dataSource: 'spoonacular' },
  { id: '3', title: 'Beef Tacos', image: '', cuisines: ['Mexican'], readyInMinutes: 25, likes: 820, dataSource: 'community' },
  { id: '4', title: 'Pad Thai', image: '', cuisines: ['Thai'], readyInMinutes: 35, likes: 760, dataSource: 'spoonacular' },
  { id: '5', title: 'Greek Salad', image: '', cuisines: ['Greek'], readyInMinutes: 10, likes: 640, dataSource: 'community' },
];

const MOCK_RECOMMENDED: SearchResult[] = [
  { id: '6', title: 'Avocado Toast', image: '', cuisines: ['American'], readyInMinutes: 10, likes: 520, dataSource: 'community' },
  { id: '7', title: 'Mushroom Risotto', image: '', cuisines: ['Italian'], readyInMinutes: 40, likes: 480, dataSource: 'spoonacular' },
  { id: '8', title: 'Miso Soup', image: '', cuisines: ['Japanese'], readyInMinutes: 15, likes: 380, dataSource: 'spoonacular' },
  { id: '9', title: 'Caesar Salad', image: '', cuisines: ['American'], readyInMinutes: 20, likes: 340, dataSource: 'community' },
];

interface ExploreViewProps {
  filters: SearchFilters;
  onFiltersChange: (filters: SearchFilters) => void;
  onSearch: (query: string) => void;
}

export function ExploreView({ filters, onFiltersChange, onSearch }: ExploreViewProps) {
  function handleSelectCuisine(cuisine: string) {
    const updated = filters.cuisines.includes(cuisine)
      ? filters.cuisines.filter((c) => c !== cuisine)
      : [...filters.cuisines, cuisine];
    onFiltersChange({ ...filters, cuisines: updated });
    // Trigger search with updated filters
    onSearch('');
  }

  function handleSelectDishType(dishType: string) {
    const updated = filters.dishTypes.includes(dishType)
      ? filters.dishTypes.filter((d) => d !== dishType)
      : [...filters.dishTypes, dishType];
    onFiltersChange({ ...filters, dishTypes: updated });
  }

  return (
    <div className="space-y-8">
      <TrendingSection recipes={MOCK_TRENDING} />
      <RecommendedSection recipes={MOCK_RECOMMENDED} />
      <CategoryShortcuts
        onSelectCuisine={handleSelectCuisine}
        onSelectDishType={handleSelectDishType}
        activeCuisines={filters.cuisines}
        activeDishTypes={filters.dishTypes}
      />
    </div>
  );
}
