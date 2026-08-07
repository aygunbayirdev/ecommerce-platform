'use client'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import type { Category } from '@/features/catalog/types'

import { CategoryAttributesDialog } from './CategoryAttributesDialog'
import { CategoryFormDialog } from './CategoryFormDialog'

function categoryLabel(category: Category, categories: Category[]): string {
  if (!category.parentCategoryId) return category.name
  const parent = categories.find((c) => c.id === category.parentCategoryId)
  return parent ? `${parent.name} / ${category.name}` : category.name
}

export function CategoryList({ categories }: { categories: Category[] }) {
  const sorted = [...categories].sort((a, b) => a.displayOrder - b.displayOrder)

  return (
    <div className="space-y-2">
      {sorted.map((category) => (
        <div key={category.id} className="flex flex-wrap items-center justify-between gap-y-2 gap-x-3 rounded-lg border p-3">
          <div className="flex items-center gap-2">
            <span className="font-medium">{categoryLabel(category, categories)}</span>
            <Badge variant="outline">Sıra: {category.displayOrder}</Badge>
            {!category.isActive && <Badge variant="destructive">Pasif</Badge>}
          </div>

          <div className="flex items-center gap-2">
            <CategoryAttributesDialog
              category={category}
              trigger={
                <Button type="button" variant="outline" size="sm">
                  Özellikler
                </Button>
              }
            />
            <CategoryFormDialog
              category={category}
              categories={categories}
              trigger={
                <Button type="button" variant="outline" size="sm">
                  Düzenle
                </Button>
              }
            />
          </div>
        </div>
      ))}
    </div>
  )
}
