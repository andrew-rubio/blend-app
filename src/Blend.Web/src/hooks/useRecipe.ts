import { useQuery } from '@tanstack/react-query'
import { getRecipe } from '@/lib/api/recipes'

export function useRecipe(id: string) {
  return useQuery({
    queryKey: ['recipe', id],
    queryFn: () => getRecipe(id),
    enabled: Boolean(id),
  })
}
