import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useReactivateProductVariant() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productId: string; variantId: string }) => {
      await apiClient.post(`/products/${payload.productId}/variants/${payload.variantId}/reactivate`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
  })
}
