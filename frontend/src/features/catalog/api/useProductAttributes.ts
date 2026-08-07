import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { ProductAttribute } from '../types'

export function useProductAttributes() {
  return useQuery({
    queryKey: ['product-attributes'],
    queryFn: async () => {
      const response = await apiClient.get<ProductAttribute[]>('/product-attributes')
      return response.data
    },
  })
}
