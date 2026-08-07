import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useRemoveCategoryAttribute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { categoryId: string; productAttributeId: string }) => {
      await apiClient.delete(`/categories/${payload.categoryId}/attributes/${payload.productAttributeId}`)
    },
    onSuccess: (_data, variables) =>
      queryClient.invalidateQueries({ queryKey: ['category-attributes', variables.categoryId] }),
  })
}
