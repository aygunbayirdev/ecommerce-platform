import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { Order } from '../types'

export function useOrder(orderId: string) {
  return useQuery({
    queryKey: ['order', orderId],
    queryFn: async () => {
      const response = await apiClient.get<Order>(`/orders/${orderId}`)
      return response.data
    },
  })
}
