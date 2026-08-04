import { serverGet } from '@/lib/server-fetch'

import type { PagedResult, ProductSummary } from '../types'

export async function getProducts(options: {
  categoryId?: string
  pageNumber?: number
  pageSize?: number
}): Promise<PagedResult<ProductSummary>> {
  const params = new URLSearchParams()
  if (options.categoryId) params.set('categoryId', options.categoryId)
  params.set('pageNumber', String(options.pageNumber ?? 1))
  params.set('pageSize', String(options.pageSize ?? 20))

  const result = await serverGet<PagedResult<ProductSummary>>(`/products?${params.toString()}`)

  return result ?? { items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0 }
}
