import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { AddressFormValues } from '../types'

export function useAddAddress() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: AddressFormValues) => {
      await apiClient.post('/addresses', { ...payload, isDefault: false })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['addresses'] }),
  })
}
