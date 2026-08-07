import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useReactivateCoupon() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (couponId: string) => {
      await apiClient.post(`/coupons/${couponId}/reactivate`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['coupons'] }),
  })
}
