'use client'

import { CategoryFormDialog } from '@/components/admin/categories/CategoryFormDialog'
import { CategoryList } from '@/components/admin/categories/CategoryList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useCategories } from '@/features/catalog/api/useCategories'

export default function AdminCategoriesPage() {
  const { data: categories, isPending, isError } = useCategories()

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Kategoriler</h1>
        <CategoryFormDialog
          categories={categories ?? []}
          trigger={<Button type="button">Yeni Kategori Ekle</Button>}
        />
      </div>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-14 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Kategoriler yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {categories && categories.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Henüz kategori yok.</p>
      )}

      {categories && categories.length > 0 && <CategoryList categories={categories} />}
    </div>
  )
}
