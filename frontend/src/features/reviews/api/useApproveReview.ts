import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useApproveReview() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (reviewId: string) => {
      await apiClient.post(`/reviews/${reviewId}/approve`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['pending-reviews'] }),
  })
}
