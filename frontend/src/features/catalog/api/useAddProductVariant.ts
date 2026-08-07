import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { ProductVariantAttributeValueInput } from '../types'

export type AddProductVariantPayload = {
  productId: string
  sku: string
  price: number
  attributeValues: ProductVariantAttributeValueInput[]
}

export function useAddProductVariant() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: AddProductVariantPayload) => {
      await apiClient.post(`/products/${payload.productId}/variants`, {
        sku: payload.sku,
        price: payload.price,
        attributeValues: payload.attributeValues,
      })
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
  })
}
