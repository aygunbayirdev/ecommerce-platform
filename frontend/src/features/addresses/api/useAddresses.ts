import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { Address } from '../types'

export function useAddresses(enabled: boolean = true) {
  return useQuery({
    queryKey: ['addresses'],
    queryFn: async () => {
      const response = await apiClient.get<Address[]>('/addresses')
      return response.data
    },
    enabled,
  })
}
