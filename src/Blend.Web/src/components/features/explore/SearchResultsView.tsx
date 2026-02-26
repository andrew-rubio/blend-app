'use client';

import { SearchResult } from '@/types/search';
import { SearchResultsGrid } from './SearchResultsGrid';
import { LoadMore } from './LoadMore';
import { EmptyState } from './EmptyState';
import { ErrorState } from './ErrorState';

interface SearchResultsViewProps {
  query: string;
  results: SearchResult[];
  totalResults: number;
  isLoading: boolean;
  isFetchingNextPage: boolean;
  hasNextPage: boolean;
  error: Error | null;
  onLoadMore: () => void;
  onRetry: () => void;
}

export function SearchResultsView({
  query,
  results,
  totalResults,
  isLoading,
  isFetchingNextPage,
  hasNextPage,
  error,
  onLoadMore,
  onRetry,
}: SearchResultsViewProps) {
  if (error) {
    return <ErrorState message={error.message} onRetry={onRetry} />;
  }

  if (!isLoading && results.length === 0) {
    return <EmptyState query={query} />;
  }

  return (
    <div>
      {!isLoading && (
        <p className="text-sm text-gray-500 mb-4">
          {totalResults} result{totalResults !== 1 ? 's' : ''} for &ldquo;{query}&rdquo;
        </p>
      )}
      <SearchResultsGrid results={results} isLoading={isLoading} />
      <LoadMore
        onLoadMore={onLoadMore}
        isLoading={isFetchingNextPage}
        hasMore={hasNextPage && !isLoading}
      />
    </div>
  );
}
