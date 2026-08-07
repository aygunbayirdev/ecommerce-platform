'use client'

import { BrandFormDialog } from '@/components/admin/brands/BrandFormDialog'
import { BrandList } from '@/components/admin/brands/BrandList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useBrands } from '@/features/catalog/api/useBrands'

export default function AdminBrandsPage() {
  const { data: brands, isPending, isError } = useBrands()

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Markalar</h1>
        <BrandFormDialog trigger={<Button type="button">Yeni Marka Ekle</Button>} />
      </div>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-14 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Markalar yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {brands && brands.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Henüz marka yok.</p>
      )}

      {brands && brands.length > 0 && <BrandList brands={brands} />}
    </div>
  )
}
