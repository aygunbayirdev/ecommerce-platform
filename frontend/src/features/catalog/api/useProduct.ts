import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { ProductDetail } from '../types'

// Client-side counterpart to the SSR getProductById — needed here because admin product
// detail is behind auth and must refetch after mutations (deactivate, add variant/image, etc.).
export function useProduct(productId: string) {
  return useQuery({
    queryKey: ['product', productId],
    queryFn: async () => {
      const response = await apiClient.get<ProductDetail>(`/products/${productId}`)
      return response.data
    },
  })
}
