import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { CreateCouponPayload } from '../types'

export function useCreateCoupon() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateCouponPayload) => {
      await apiClient.post('/coupons', payload)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['coupons'] }),
  })
}
