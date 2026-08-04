import { useMutation } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import { useAuthStore } from '../store'
import type { AuthTokens, LoginRequest } from '../types'

export function useLogin() {
  const setTokens = useAuthStore((state) => state.setTokens)

  return useMutation({
    mutationFn: async (payload: LoginRequest) => {
      const response = await apiClient.post<AuthTokens>('/auth/login', payload)
      return response.data
    },
    onSuccess: setTokens,
  })
}
