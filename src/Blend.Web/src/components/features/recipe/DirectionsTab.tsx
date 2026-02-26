import type { Recipe } from '@/types/recipe'
import Image from 'next/image'

interface Props { recipe: Recipe }

export function DirectionsTab({ recipe }: Props) {
  return (
    <ol className="space-y-6">
      {recipe.steps.map((step) => (
        <li key={step.number} className="flex gap-4">
          <span className="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-orange-500 text-sm font-bold text-white">
            {step.number}
          </span>
          <div className="flex-1">
            <p className="text-gray-700">{step.description}</p>
            {step.imageUrl && (
              <div className="mt-3 relative h-48 overflow-hidden rounded-lg">
                <Image src={step.imageUrl} alt={`Step ${step.number}`} fill className="object-cover" />
              </div>
            )}
          </div>
        </li>
      ))}
    </ol>
  )
}
