import Link from 'next/link'

import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import type { OrderSummary } from '@/features/orders/types'
import { formatPrice } from '@/lib/format'
import { orderStatusLabel } from '@/lib/orderStatus'

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' })
}

export function OrderListItem({ order }: { order: OrderSummary }) {
  return (
    <Link href={`/orders/${order.id}`}>
      <Card className="transition-shadow hover:shadow-md">
        <CardContent className="flex items-center justify-between gap-4">
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
        </CardContent>
      </Card>
    </Link>
  )
}
