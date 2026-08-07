import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export type CreateProductPayload = {
  categoryId: string
  brandId: string | null
  name: string
  description: string
}

export function useCreateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateProductPayload) => {
      const response = await apiClient.post<{ id: string }>('/products', payload)
      return response.data.id
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-products'] }),
  })
}
