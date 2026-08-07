import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export type UpdateCategoryPayload = {
  categoryId: string
  name: string
  displayOrder: number
}

export function useUpdateCategory() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: UpdateCategoryPayload) => {
      await apiClient.put(`/categories/${payload.categoryId}`, {
        name: payload.name,
        displayOrder: payload.displayOrder,
      })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['categories'] }),
  })
}
