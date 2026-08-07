'use client'

import { useState } from 'react'

import { ProductFormDialog } from '@/components/admin/products/ProductFormDialog'
import { ProductList } from '@/components/admin/products/ProductList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useAdminProducts } from '@/features/catalog/api/useAdminProducts'
import { useCategories } from '@/features/catalog/api/useCategories'

const nativeSelectClassName =
  'h-8 min-w-48 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30'

export default function AdminProductsPage() {
  const [categoryId, setCategoryId] = useState('')
  const [pageNumber, setPageNumber] = useState(1)

  const { data: categories } = useCategories()
  const { data, isPending, isError } = useAdminProducts(categoryId || undefined, pageNumber)

  function handleCategoryChange(value: string) {
    setCategoryId(value)
    setPageNumber(1)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Ürünler</h1>
        <ProductFormDialog
          categories={categories ?? []}
          trigger={<Button type="button">Yeni Ürün Ekle</Button>}
        />
      </div>

      <select
        className={nativeSelectClassName}
        value={categoryId}
        onChange={(event) => handleCategoryChange(event.target.value)}
      >
        <option value="">Tüm kategoriler</option>
        {(categories ?? []).map((category) => (
          <option key={category.id} value={category.id}>
            {category.name}
          </option>
        ))}
      </select>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Ürünler yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {data && data.items.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Ürün bulunamadı.</p>
      )}

      {data && data.items.length > 0 && <ProductList products={data.items} categories={categories ?? []} />}

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-3">
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={pageNumber <= 1}
            onClick={() => setPageNumber((page) => page - 1)}
          >
            Önceki
          </Button>
          <span className="text-sm text-muted-foreground">
            {pageNumber} / {data.totalPages}
          </span>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={pageNumber >= data.totalPages}
            onClick={() => setPageNumber((page) => page + 1)}
          >
            Sonraki
          </Button>
        </div>
      )}
    </div>
  )
}
