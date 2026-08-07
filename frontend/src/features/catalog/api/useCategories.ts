import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { Category } from '../types'

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: async () => {
      const response = await apiClient.get<Category[]>('/categories')
      return response.data
    },
  })
}
