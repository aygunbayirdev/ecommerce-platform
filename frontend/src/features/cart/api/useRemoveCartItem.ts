import { useMutation, useQueryClient } from '@tanstack/react-query'

import { useAuthStore } from '@/features/auth/store'
import { apiClient } from '@/lib/axios'

import { cartBasePath } from './cartBasePath'
import { useGuestCartStore } from '../store'

export function useRemoveCartItem() {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const guestCartId = useGuestCartStore((state) => state.guestCartId)
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (productVariantId: string) => {
      const basePath = cartBasePath(isAuthenticated, guestCartId)
      if (!basePath) throw new Error('No cart to update')

      await apiClient.delete(`${basePath}/items/${productVariantId}`)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })
}
