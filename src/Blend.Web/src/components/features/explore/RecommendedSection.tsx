'use client';

import Link from 'next/link';
import Image from 'next/image';
import { SearchResult } from '@/types/search';
import { SkeletonCard } from '@/components/ui/SkeletonCard';
import { Badge } from '@/components/ui/Badge';

interface RecommendedCardProps {
  recipe: SearchResult;
}

function RecommendedCard({ recipe }: RecommendedCardProps) {
  return (
    <Link
      href={`/recipes/${recipe.id}`}
      className="rounded-xl overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow"
    >
      <div className="relative h-36 w-full bg-gray-100">
        {recipe.image ? (
          <Image
            src={recipe.image}
            alt={recipe.title}
            fill
            className="object-cover"
            sizes="(max-width: 640px) 50vw, 33vw"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gray-200">
            <span className="text-3xl">🍽️</span>
          </div>
        )}
      </div>
      <div className="p-3">
        <p className="text-sm font-medium text-gray-800 line-clamp-2">{recipe.title}</p>
        <div className="flex items-center justify-between mt-2">
          <p className="text-xs text-gray-500">⏱ {recipe.readyInMinutes} min</p>
          <Badge source={recipe.dataSource} />
        </div>
      </div>
    </Link>
  );
}

interface RecommendedSectionProps {
  recipes?: SearchResult[];
  isLoading?: boolean;
}

export function RecommendedSection({ recipes, isLoading }: RecommendedSectionProps) {
  return (
    <section aria-labelledby="recommended-heading">
      <h2 id="recommended-heading" className="text-lg font-semibold text-gray-900 mb-3">
        Recommended for you
      </h2>
      <div className="grid grid-cols-2 gap-3">
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => <SkeletonCard key={i} />)
          : recipes?.map((recipe) => <RecommendedCard key={recipe.id} recipe={recipe} />)}
      </div>
    </section>
  );
}
