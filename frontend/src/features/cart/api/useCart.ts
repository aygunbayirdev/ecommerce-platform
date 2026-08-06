import { useQuery } from '@tanstack/react-query'

import { useAuthStore } from '@/features/auth/store'
import { apiClient } from '@/lib/axios'

import { cartBasePath } from './cartBasePath'
import { useGuestCartStore } from '../store'
import type { Cart } from '../types'

export function useCart() {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const guestCartId = useGuestCartStore((state) => state.guestCartId)
  const cartPath = cartBasePath(isAuthenticated, guestCartId)

  return useQuery({
    queryKey: ['cart', cartPath],
    queryFn: async () => {
      const response = await apiClient.get<Cart>(cartPath!)
      return response.data
    },
    enabled: cartPath !== null,
  })
}
