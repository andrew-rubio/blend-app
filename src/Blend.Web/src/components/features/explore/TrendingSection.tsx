'use client';

import Link from 'next/link';
import Image from 'next/image';
import { SearchResult } from '@/types/search';
import { SkeletonCard } from '@/components/ui/SkeletonCard';

interface TrendingCardProps {
  recipe: SearchResult;
}

function TrendingCard({ recipe }: TrendingCardProps) {
  return (
    <Link
      href={`/recipes/${recipe.id}`}
      className="flex-shrink-0 w-44 rounded-xl overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow"
    >
      <div className="relative h-28 w-full bg-gray-100">
        {recipe.image ? (
          <Image
            src={recipe.image}
            alt={recipe.title}
            fill
            className="object-cover"
            sizes="176px"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gray-200">
            <span className="text-3xl">🍽️</span>
          </div>
        )}
      </div>
      <div className="p-2">
        <p className="text-xs font-medium text-gray-800 line-clamp-2">{recipe.title}</p>
        <p className="text-xs text-gray-500 mt-1">❤️ {recipe.likes}</p>
      </div>
    </Link>
  );
}

interface TrendingSectionProps {
  recipes?: SearchResult[];
  isLoading?: boolean;
}

export function TrendingSection({ recipes, isLoading }: TrendingSectionProps) {
  return (
    <section aria-labelledby="trending-heading">
      <h2 id="trending-heading" className="text-lg font-semibold text-gray-900 mb-3">
        Trending recipes
      </h2>
      <div className="flex gap-3 overflow-x-auto pb-2 scrollbar-hide -mx-4 px-4">
        {isLoading
          ? Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex-shrink-0 w-44">
                <SkeletonCard />
              </div>
            ))
          : recipes?.map((recipe) => <TrendingCard key={recipe.id} recipe={recipe} />)}
      </div>
    </section>
  );
}
