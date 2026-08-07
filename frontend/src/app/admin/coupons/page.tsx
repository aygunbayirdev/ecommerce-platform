'use client'

import { useState } from 'react'

import { CouponFormDialog } from '@/components/admin/coupons/CouponFormDialog'
import { CouponList } from '@/components/admin/coupons/CouponList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useCoupons } from '@/features/coupons/api/useCoupons'

export default function AdminCouponsPage() {
  const [pageNumber, setPageNumber] = useState(1)
  const { data, isPending, isError } = useCoupons(pageNumber)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Kuponlar</h1>
        <CouponFormDialog trigger={<Button type="button">Yeni Kupon Ekle</Button>} />
      </div>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Kuponlar yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {data && data.items.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Henüz kupon yok.</p>
      )}

      {data && data.items.length > 0 && <CouponList coupons={data.items} />}

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
