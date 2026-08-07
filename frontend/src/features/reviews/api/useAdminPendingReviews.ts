import { useQuery } from '@tanstack/react-query'

import type { PagedResult } from '@/features/catalog/types'
import { apiClient } from '@/lib/axios'

import type { ReviewAdmin } from '../types'

export function useAdminPendingReviews(pageNumber: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['pending-reviews', pageNumber, pageSize],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<ReviewAdmin>>('/reviews/pending', {
        params: { pageNumber, pageSize },
      })
      return response.data
    },
  })
}
