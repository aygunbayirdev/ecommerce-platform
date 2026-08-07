import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useCreateBrand() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (name: string) => {
      await apiClient.post('/brands', { name })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['brands'] }),
  })
}
