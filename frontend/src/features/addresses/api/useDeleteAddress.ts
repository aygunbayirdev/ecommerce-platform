import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useDeleteAddress() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (addressId: string) => {
      await apiClient.delete(`/addresses/${addressId}`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['addresses'] }),
  })
}
