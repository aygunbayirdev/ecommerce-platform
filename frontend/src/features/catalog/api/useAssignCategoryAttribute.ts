import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useAssignCategoryAttribute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { categoryId: string; productAttributeId: string }) => {
      await apiClient.post(`/categories/${payload.categoryId}/attributes`, {
        productAttributeId: payload.productAttributeId,
      })
    },
    onSuccess: (_data, variables) =>
      queryClient.invalidateQueries({ queryKey: ['category-attributes', variables.categoryId] }),
  })
}
