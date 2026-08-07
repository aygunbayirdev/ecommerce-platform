import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { CreateOrderRequest } from '../types'

export function useCreateOrder() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateOrderRequest) => {
      const response = await apiClient.post<{ id: string }>('/orders', payload)
      return response.data
    },
    // The backend clears the cart as part of checkout orchestration — invalidate so the header
    // badge and cart page reflect that immediately instead of showing stale items.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })
}
