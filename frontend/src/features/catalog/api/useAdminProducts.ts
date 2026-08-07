import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { PagedResult, ProductSummary } from '../types'

export function useAdminProducts(categoryId: string | undefined, pageNumber: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['admin-products', categoryId, pageNumber, pageSize],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<ProductSummary>>('/products/all', {
        params: { categoryId, pageNumber, pageSize },
      })
      return response.data
    },
  })
}
