import { serverGet } from '@/lib/server-fetch'

import type { ProductDetail } from '../types'

export async function getProductById(productId: string): Promise<ProductDetail | null> {
  return serverGet<ProductDetail>(`/products/${productId}`)
}
