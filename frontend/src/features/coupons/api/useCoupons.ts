import { useQuery } from '@tanstack/react-query'

import type { PagedResult } from '@/features/catalog/types'
import { apiClient } from '@/lib/axios'

import type { Coupon } from '../types'

export function useCoupons(pageNumber: number, pageSize: number = 20) {
  return useQuery({
    queryKey: ['coupons', pageNumber, pageSize],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<Coupon>>('/coupons', {
        params: { pageNumber, pageSize },
      })
      return response.data
    },
  })
}
