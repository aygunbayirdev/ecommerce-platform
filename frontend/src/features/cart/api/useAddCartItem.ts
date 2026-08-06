import { useMutation, useQueryClient } from '@tanstack/react-query'

import { useAuthStore } from '@/features/auth/store'
import { apiClient } from '@/lib/axios'

import { useGuestCartStore } from '../store'

export function useAddCartItem() {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const guestCartId = useGuestCartStore((state) => state.guestCartId)
  const setGuestCartId = useGuestCartStore((state) => state.setGuestCartId)
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { productVariantId: string; quantity: number }) => {
      if (isAuthenticated) {
        await apiClient.post('/carts/mine/items', payload)
        return
      }

      // Guest carts don't exist until the first item is added — create one lazily and remember
      // its id so pure browsers who never buy anything never leave an empty cart behind.
      let cartId = guestCartId
      if (!cartId) {
        const { data } = await apiClient.post<{ id: string }>('/carts')
        cartId = data.id
        setGuestCartId(cartId)
      }

      await apiClient.post(`/carts/${cartId}/items`, payload)
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })
}
