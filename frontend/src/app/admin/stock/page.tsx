'use client'

import { useState } from 'react'

import { StockTable } from '@/components/admin/stock/StockTable'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useStockItems } from '@/features/inventory/api/useStockItems'

export default function AdminStockPage() {
  const [pageNumber, setPageNumber] = useState(1)
  const { data, isPending, isError } = useStockItems(pageNumber)

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold tracking-tight">Stok</h1>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Stok listesi yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {data && <StockTable items={data.items} />}

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
