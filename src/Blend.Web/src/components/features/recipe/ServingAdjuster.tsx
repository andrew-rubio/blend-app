interface Props {
  servings: number
  onServingsChange: (servings: number) => void
}

export function ServingAdjuster({ servings, onServingsChange }: Props) {
  return (
    <div className="flex items-center gap-3">
      <span className="text-sm text-gray-500">Servings:</span>
      <div className="flex items-center gap-2">
        <button
          aria-label="Decrease servings"
          onClick={() => onServingsChange(Math.max(1, servings - 1))}
          disabled={servings <= 1}
          className="flex h-8 w-8 items-center justify-center rounded-full border border-gray-300 text-gray-600 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-40"
        >
          -
        </button>
        <span className="w-6 text-center text-sm font-medium">{servings}</span>
        <button
          aria-label="Increase servings"
          onClick={() => onServingsChange(servings + 1)}
          className="flex h-8 w-8 items-center justify-center rounded-full border border-gray-300 text-gray-600 hover:bg-gray-100"
        >
          +
        </button>
      </div>
    </div>
  )
}
