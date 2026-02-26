interface BadgeProps {
  source: 'spoonacular' | 'community';
}

export function Badge({ source }: BadgeProps) {
  const isSpoonacular = source === 'spoonacular';
  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
        isSpoonacular
          ? 'bg-green-100 text-green-800'
          : 'bg-purple-100 text-purple-800'
      }`}
    >
      {isSpoonacular ? 'Spoonacular' : 'Community'}
    </span>
  );
}
