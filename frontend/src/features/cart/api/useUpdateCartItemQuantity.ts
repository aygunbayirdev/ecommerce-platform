import { useMutation, useQueryClient } from '@tanstack/react-query'

import { useAuthStore } from '@/features/auth/store'
import { apiClient } from '@/lib/axios'

import { cartBasePath } from './cartBasePath'
import { useGuestCartStore } from '../store'

export function useUpdateCartItemQuantity() {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const guestCartId = useGuestCartStore((state) => state.guestCartId)
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productVariantId: string; quantity: number }) => {
      const basePath = cartBasePath(isAuthenticated, guestCartId)
      if (!basePath) throw new Error('No cart to update')

      await apiClient.put(`${basePath}/items/${payload.productVariantId}`, { quantity: payload.quantity })
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })
}
