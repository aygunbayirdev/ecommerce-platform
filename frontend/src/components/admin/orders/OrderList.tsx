import Link from 'next/link'

import { Badge } from '@/components/ui/badge'
import type { OrderSummary } from '@/features/orders/types'
import { formatPrice } from '@/lib/format'
import { orderStatusLabel } from '@/lib/orderStatus'

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' })
}

export function OrderList({ orders }: { orders: OrderSummary[] }) {
  return (
    <div className="space-y-2">
      {orders.map((order) => (
        <Link
          key={order.id}
          href={`/admin/orders/${order.id}`}
          className="flex flex-wrap items-center justify-between gap-y-2 gap-x-4 rounded-lg border p-3 transition-colors hover:bg-muted"
        >
          <div className="space-y-1">
            <p className="font-medium">#{order.orderNumber}</p>
            <p className="text-sm text-muted-foreground">{formatDate(order.createdAtUtc)}</p>
          </div>
          <div className="flex items-center gap-3">
            <Badge variant={order.status === 'Cancelled' ? 'destructive' : 'secondary'}>
              {orderStatusLabel(order.status)}
            </Badge>
            <span className="font-medium">{formatPrice(order.total)}</span>
          </div>
        </Link>
      ))}
    </div>
  )
}
