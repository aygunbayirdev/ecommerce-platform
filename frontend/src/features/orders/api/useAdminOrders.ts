import { useQuery } from '@tanstack/react-query'

import type { PagedResult } from '@/features/catalog/types'
import { apiClient } from '@/lib/axios'

import type { OrderStatus, OrderSummary } from '../types'

export function useAdminOrders(status: OrderStatus | undefined, pageNumber: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['admin-orders', status, pageNumber, pageSize],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<OrderSummary>>('/orders/all', {
        params: { status, pageNumber, pageSize },
      })
      return response.data
    },
  })
}
