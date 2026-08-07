import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useIncreaseStock() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productVariantId: string; quantity: number; reason?: string }) => {
      await apiClient.post(`/stock-items/${payload.productVariantId}/increase`, {
        quantity: payload.quantity,
        reason: payload.reason ?? null,
      })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['stock-items'] }),
  })
}
