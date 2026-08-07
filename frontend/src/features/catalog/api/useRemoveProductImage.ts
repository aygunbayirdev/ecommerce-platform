import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useRemoveProductImage() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productId: string; imageId: string }) => {
      await apiClient.delete(`/products/${payload.productId}/images/${payload.imageId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
  })
}
