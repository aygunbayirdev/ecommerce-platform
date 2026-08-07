import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export type CreateCategoryPayload = {
  name: string
  parentCategoryId: string | null
  displayOrder: number
}

export function useCreateCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateCategoryPayload) => {
      await apiClient.post('/categories', payload)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['categories'] }),
  })
}
