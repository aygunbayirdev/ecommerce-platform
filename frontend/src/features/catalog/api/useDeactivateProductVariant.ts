import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useDeactivateProductVariant() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productId: string; variantId: string }) => {
      await apiClient.post(`/products/${payload.productId}/variants/${payload.variantId}/deactivate`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
  })
}
