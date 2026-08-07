import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useDeactivateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (productId: string) => {
      await apiClient.post(`/products/${productId}/deactivate`)
    },
    onSuccess: (_data, productId) => {
      queryClient.invalidateQueries({ queryKey: ['product', productId] })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
  })
}
