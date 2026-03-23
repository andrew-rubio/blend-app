'use client'

import { useCallback } from 'react'
import { useHome, useRefreshHome } from '@/hooks/useHome'
import { HomeSearchBar } from './HomeSearchBar'
import { FeaturedCarousel, FeaturedCarouselSkeleton } from './FeaturedCarousel'
import { FeaturedStoriesSection, FeaturedStoriesSkeleton } from './FeaturedStoriesSection'
import { CommunityRecipesGrid, CommunityRecipesGridSkeleton } from './CommunityRecipesGrid'
import { FeaturedVideosSection, FeaturedVideosSkeleton } from './FeaturedVideosSection'
import { RecentlyViewedSection } from './RecentlyViewedSection'
import { PullToRefresh } from './PullToRefresh'

export function HomeContainer() {
  const { data, isLoading, error } = useHome()
  const refreshHome = useRefreshHome()

  const handleRefresh = useCallback(async () => {
    await refreshHome()
  }, [refreshHome])

  return (
    <PullToRefresh onRefresh={handleRefresh}>
      <div className="mx-auto flex max-w-7xl flex-col gap-8 px-4 py-6 sm:px-6 lg:px-8">
        {/* Search bar */}
        <HomeSearchBar
          initialPlaceholder={data?.search?.placeholder}
        />

        {/* Error state */}
        {error && (
          <div role="alert" className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/20 dark:text-red-400">
            Could not load home content. Please try again.
          </div>
        )}

        {/* Featured recipes carousel */}
        {isLoading ? (
          <FeaturedCarouselSkeleton />
        ) : (
          data?.featured?.recipes && data.featured.recipes.length > 0 && (
            <FeaturedCarousel recipes={data.featured.recipes} />
          )
        )}

        {/* Featured stories */}
        {isLoading ? (
          <FeaturedStoriesSkeleton />
        ) : (
          data?.featured?.stories && <FeaturedStoriesSection stories={data.featured.stories} />
        )}

        {/* Featured videos */}
        {isLoading ? (
          <FeaturedVideosSkeleton />
        ) : (
          data?.featured?.videos && <FeaturedVideosSection videos={data.featured.videos} />
        )}

        {/* Community recipes */}
        {isLoading ? (
          <CommunityRecipesGridSkeleton />
        ) : (
          data?.community?.recipes && <CommunityRecipesGrid recipes={data.community.recipes} />
        )}

        {/* Recently viewed (authenticated users only, hidden when empty) */}
        {!isLoading && data?.recentlyViewed?.recipes && data.recentlyViewed.recipes.length > 0 && (
          <RecentlyViewedSection recipes={data.recentlyViewed.recipes} />
        )}
      </div>
    </PullToRefresh>
  )
}
