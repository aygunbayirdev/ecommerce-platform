import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useDeactivateCoupon() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (couponId: string) => {
      await apiClient.post(`/coupons/${couponId}/deactivate`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['coupons'] }),
  })
}
