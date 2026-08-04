import type { PagedResult } from '@/features/catalog/types'
import { serverGet } from '@/lib/server-fetch'

import type { Review } from '../types'

export async function getApprovedReviews(productId: string): Promise<PagedResult<Review>> {
  const result = await serverGet<PagedResult<Review>>(`/reviews/by-product/${productId}`)
  return result ?? { items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 }
}
