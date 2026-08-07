import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { ProductAttribute } from '../types'

export function useCategoryAttributes(categoryId: string | undefined) {
  return useQuery({
    queryKey: ['category-attributes', categoryId],
    queryFn: async () => {
      const response = await apiClient.get<ProductAttribute[]>(`/categories/${categoryId}/attributes`)
      return response.data
    },
    enabled: !!categoryId,
  })
}
