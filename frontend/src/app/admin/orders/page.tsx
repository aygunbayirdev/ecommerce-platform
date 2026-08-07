'use client'

import { useState } from 'react'

import { OrderList } from '@/components/admin/orders/OrderList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useAdminOrders } from '@/features/orders/api/useAdminOrders'
import type { OrderStatus } from '@/features/orders/types'
import { orderStatusLabel } from '@/lib/orderStatus'

const nativeSelectClassName =
  'h-8 min-w-48 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30'

const ALL_STATUSES: OrderStatus[] = [
  'Created',
  'PaymentPending',
  'Paid',
  'Preparing',
  'Shipped',
  'Delivered',
  'Cancelled',
]

export default function AdminOrdersPage() {
  const [status, setStatus] = useState<OrderStatus | ''>('')
  const [pageNumber, setPageNumber] = useState(1)

  const { data, isPending, isError } = useAdminOrders(status || undefined, pageNumber)

  function handleStatusChange(value: string) {
    setStatus(value as OrderStatus | '')
    setPageNumber(1)
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold tracking-tight">Siparişler</h1>

      <select className={nativeSelectClassName} value={status} onChange={(event) => handleStatusChange(event.target.value)}>
        <option value="">Tüm durumlar</option>
        {ALL_STATUSES.map((s) => (
          <option key={s} value={s}>
            {orderStatusLabel(s)}
          </option>
        ))}
      </select>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Siparişler yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {data && data.items.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Sipariş bulunamadı.</p>
      )}

      {data && data.items.length > 0 && <OrderList orders={data.items} />}

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
