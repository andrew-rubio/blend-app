'use client';

import Link from 'next/link';
import Image from 'next/image';
import { SearchResult } from '@/types/search';
import { Badge } from '@/components/ui/Badge';

interface SearchResultCardProps {
  recipe: SearchResult;
}

export function SearchResultCard({ recipe }: SearchResultCardProps) {
  return (
    <Link
      href={`/recipes/${recipe.id}`}
      className="rounded-xl overflow-hidden bg-white shadow-sm hover:shadow-md transition-shadow"
    >
      <div className="relative h-40 w-full bg-gray-100">
        {recipe.image ? (
          <Image
            src={recipe.image}
            alt={recipe.title}
            fill
            className="object-cover"
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gray-200">
            <span className="text-4xl">🍽️</span>
          </div>
        )}
      </div>
      <div className="p-3">
        <h3 className="text-sm font-semibold text-gray-800 line-clamp-2 mb-1">{recipe.title}</h3>
        {recipe.cuisines.length > 0 && (
          <div className="flex flex-wrap gap-1 mb-2">
            {recipe.cuisines.slice(0, 2).map((c) => (
              <span key={c} className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
                {c}
              </span>
            ))}
          </div>
        )}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3 text-xs text-gray-500">
            <span>⏱ {recipe.readyInMinutes} min</span>
            <span>❤️ {recipe.likes}</span>
          </div>
          <Badge source={recipe.dataSource} />
        </div>
      </div>
    </Link>
  );
}
