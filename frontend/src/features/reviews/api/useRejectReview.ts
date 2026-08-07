import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useRejectReview() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (reviewId: string) => {
      await apiClient.delete(`/reviews/${reviewId}`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['pending-reviews'] }),
  })
}
