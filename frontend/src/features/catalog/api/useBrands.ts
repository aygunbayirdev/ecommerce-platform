import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { Brand } from '../types'

export function useBrands() {
  return useQuery({
    queryKey: ['brands'],
    queryFn: async () => {
      const response = await apiClient.get<Brand[]>('/brands')
      return response.data
    },
  })
}
