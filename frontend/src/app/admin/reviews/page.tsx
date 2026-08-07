'use client'

import { useState } from 'react'

import { PendingReviewList } from '@/components/admin/reviews/PendingReviewList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useAdminPendingReviews } from '@/features/reviews/api/useAdminPendingReviews'

export default function AdminReviewsPage() {
  const [pageNumber, setPageNumber] = useState(1)
  const { data, isPending, isError } = useAdminPendingReviews(pageNumber)

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold tracking-tight">Yorumlar</h1>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Yorumlar yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {data && data.items.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Bekleyen yorum yok.</p>
      )}

      {data && data.items.length > 0 && <PendingReviewList reviews={data.items} />}

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
