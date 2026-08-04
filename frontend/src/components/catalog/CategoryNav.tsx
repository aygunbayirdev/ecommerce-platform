import Link from 'next/link'

import { Badge } from '@/components/ui/badge'
import type { Category } from '@/features/catalog/types'

export function CategoryNav({ categories, activeCategoryId }: { categories: Category[]; activeCategoryId?: string }) {
  const topLevel = categories
    .filter((category) => category.parentCategoryId === null && category.isActive)
    .sort((a, b) => a.displayOrder - b.displayOrder)

  if (topLevel.length === 0) {
    return null
  }

  return (
    <nav className="flex flex-wrap gap-2">
      {topLevel.map((category) => (
        <Link key={category.id} href={`/category/${category.id}`}>
          <Badge variant={category.id === activeCategoryId ? 'default' : 'secondary'}>
            {category.name}
          </Badge>
        </Link>
      ))}
    </nav>
  )
}
