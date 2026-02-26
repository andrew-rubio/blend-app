import { SkeletonCard } from '@/components/ui/SkeletonCard';

export default function ExploreLoading() {
  return (
    <main className="min-h-screen bg-gray-50">
      <div className="max-w-2xl mx-auto px-4 py-6">
        <div className="h-8 w-32 bg-gray-200 rounded animate-pulse mb-4" />
        <div className="h-12 w-full bg-gray-200 rounded-xl animate-pulse mb-6" />
        <div className="grid grid-cols-2 gap-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      </div>
    </main>
  );
}
