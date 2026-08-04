import { useMutation } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { RegisterRequest } from '../types'

export function useRegister() {
  return useMutation({
    mutationFn: async (payload: RegisterRequest) => {
      const response = await apiClient.post<{ id: string }>('/auth/register', payload)
      return response.data
    },
  })
}
