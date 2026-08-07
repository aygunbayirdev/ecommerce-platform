import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useMarkOrderAsPreparing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (orderId: string) => {
      await apiClient.post(`/orders/${orderId}/mark-preparing`)
    },
    onSuccess: (_data, orderId) => {
      queryClient.invalidateQueries({ queryKey: ['order', orderId] })
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] })
    },
  })
}
