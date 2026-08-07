import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useUpdateBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { brandId: string; name: string }) => {
      await apiClient.put(`/brands/${payload.brandId}`, { name: payload.name })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['brands'] }),
  })
}
