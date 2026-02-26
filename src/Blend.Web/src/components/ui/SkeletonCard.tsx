export function SkeletonCard() {
  return (
    <div className="animate-pulse rounded-xl overflow-hidden bg-white shadow-sm">
      <div className="bg-gray-200 h-40 w-full" />
      <div className="p-3 space-y-2">
        <div className="bg-gray-200 h-4 rounded w-3/4" />
        <div className="bg-gray-200 h-3 rounded w-1/2" />
        <div className="flex gap-2 mt-2">
          <div className="bg-gray-200 h-5 rounded-full w-16" />
          <div className="bg-gray-200 h-5 rounded-full w-16" />
        </div>
      </div>
    </div>
  );
}
