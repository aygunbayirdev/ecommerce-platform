import { serverGet } from '@/lib/server-fetch'

import type { Category } from '../types'

export async function getCategories(): Promise<Category[]> {
  const result = await serverGet<Category[]>('/categories')
  return result ?? []
}
