import { useQuery } from '@tanstack/react-query'

import type { PagedResult } from '@/features/catalog/types'
import { apiClient } from '@/lib/axios'

import type { OrderSummary } from '../types'

export function useMyOrders(pageNumber: number, pageSize: number = 10, enabled: boolean = true) {
  return useQuery({
    queryKey: ['orders', pageNumber, pageSize],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<OrderSummary>>('/orders/mine', {
        params: { pageNumber, pageSize },
      })
      return response.data
    },
    enabled,
  })
}
