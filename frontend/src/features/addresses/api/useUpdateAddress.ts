import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { AddressFormValues } from '../types'

export function useUpdateAddress() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { addressId: string; values: AddressFormValues }) => {
      await apiClient.put(`/addresses/${payload.addressId}`, payload.values)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['addresses'] }),
  })
}
