import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useAddProductImage() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productId: string; url: string; isPrimary: boolean }) => {
      await apiClient.post(`/products/${payload.productId}/images`, {
        url: payload.url,
        isPrimary: payload.isPrimary,
      })
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
  })
}
