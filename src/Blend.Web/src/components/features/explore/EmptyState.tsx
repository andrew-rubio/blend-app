'use client';

interface EmptyStateProps {
  query: string;
}

export function EmptyState({ query }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <span className="text-6xl mb-4">🔍</span>
      <h3 className="text-lg font-semibold text-gray-800 mb-2">No results found</h3>
      <p className="text-gray-500 text-sm mb-4">
        We couldn&apos;t find any recipes matching &ldquo;{query}&rdquo;.
      </p>
      <ul className="text-sm text-gray-500 space-y-1 text-left list-disc list-inside">
        <li>Check your spelling</li>
        <li>Try more general keywords</li>
        <li>Remove some filters</li>
      </ul>
    </div>
  );
}
