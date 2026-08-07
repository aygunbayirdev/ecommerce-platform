import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useCreateProductAttribute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (name: string) => {
      await apiClient.post('/product-attributes', { name })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['product-attributes'] }),
  })
}
