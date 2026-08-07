import { useMutation } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { CreateReviewRequest } from '../types'

export function useCreateReview() {
  return useMutation({
    mutationFn: async (payload: CreateReviewRequest) => {
      const response = await apiClient.post<{ id: string }>('/reviews', payload)
      return response.data
    },
  })
}
