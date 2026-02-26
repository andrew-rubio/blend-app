import Image from 'next/image'
import type { Recipe } from '@/types/recipe'

interface DirectionsTabProps {
  steps: Recipe['steps']
}

export function DirectionsTab({ steps }: DirectionsTabProps) {
  return (
    <ol className="space-y-6">
      {steps.map((step) => (
        <li key={step.number} className="flex gap-4">
          <div className="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-orange-500 text-sm font-bold text-white">
            {step.number}
          </div>
          <div className="flex-1">
            <p className="text-gray-800 leading-relaxed">{step.description}</p>
            {step.imageUrl && (
              <div className="relative mt-3 h-48 w-full overflow-hidden rounded-lg">
                <Image src={step.imageUrl} alt={`Step ${step.number}`} fill className="object-cover" />
              </div>
            )}
          </div>
        </li>
      ))}
    </ol>
  )
}
