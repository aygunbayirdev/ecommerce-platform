import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useAdminCancelOrder() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { orderId: string; reason: string }) => {
      await apiClient.post(`/orders/${payload.orderId}/admin-cancel`, { reason: payload.reason })
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['order', variables.orderId] })
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] })
    },
  })
}
