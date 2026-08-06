import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useSetDefaultAddress() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (addressId: string) => {
      await apiClient.put(`/addresses/${addressId}/default`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['addresses'] }),
  })
}
