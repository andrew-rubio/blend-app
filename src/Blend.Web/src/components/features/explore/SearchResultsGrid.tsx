'use client';

import { SearchResult } from '@/types/search';
import { SearchResultCard } from './SearchResultCard';
import { SkeletonCard } from '@/components/ui/SkeletonCard';

interface SearchResultsGridProps {
  results: SearchResult[];
  isLoading?: boolean;
}

export function SearchResultsGrid({ results, isLoading }: SearchResultsGridProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      {results.map((recipe) => (
        <SearchResultCard key={recipe.id} recipe={recipe} />
      ))}
    </div>
  );
}
