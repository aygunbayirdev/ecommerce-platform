import { useQuery } from '@tanstack/react-query'

import type { PagedResult } from '@/features/catalog/types'
import { apiClient } from '@/lib/axios'

import type { StockItemWithProduct } from '../types'

export function useStockItems(pageNumber: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['stock-items', pageNumber, pageSize],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<StockItemWithProduct>>('/stock-items', {
        params: { pageNumber, pageSize },
      })
      return response.data
    },
  })
}
