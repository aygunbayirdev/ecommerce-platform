'use client'

import Link from 'next/link'
import { useState } from 'react'

import { OrderListItem } from '@/components/orders/OrderListItem'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useAuthStore } from '@/features/auth/store'
import { useMyOrders } from '@/features/orders/api/useMyOrders'

export default function OrdersPage() {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const [pageNumber, setPageNumber] = useState(1)
  const { data, isPending, isError } = useMyOrders(pageNumber, 10, isAuthenticated)

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-3xl space-y-4 px-4 py-16 text-center">
        <p className="text-muted-foreground">Bu sayfayı görüntülemek için giriş yapmalısınız.</p>
        <Button render={<Link href="/login" />} nativeButton={false}>
          Giriş Yap
        </Button>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold tracking-tight">Siparişlerim</h1>

      {isPending && (
        <div className="space-y-3">
          <Skeleton className="h-20 w-full" />
          <Skeleton className="h-20 w-full" />
        </div>
      )}

      {isError && (
        <p className="text-sm text-destructive">Siparişler yüklenemedi. Lütfen sayfayı yenileyin.</p>
      )}

      {data && data.items.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Henüz siparişiniz yok.</p>
      )}

      {data && data.items.length > 0 && (
        <div className="space-y-3">
          {data.items.map((order) => (
            <OrderListItem key={order.id} order={order} />
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-3">
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
