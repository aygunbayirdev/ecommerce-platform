'use client'

import { useParams } from 'next/navigation'

import { OrderStatusActions } from '@/components/admin/orders/OrderStatusActions'
import { ShipmentActions } from '@/components/admin/orders/ShipmentActions'
import { OrderItemsList } from '@/components/checkout/OrderItemsList'
import { OrderStatusHistoryTimeline } from '@/components/orders/OrderStatusHistoryTimeline'
import { ShipmentInfo } from '@/components/orders/ShipmentInfo'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { useOrder } from '@/features/orders/api/useOrder'
import { useShipment } from '@/features/shipments/api/useShipment'
import { orderStatusLabel } from '@/lib/orderStatus'

export default function AdminOrderDetailPage() {
  const { orderId } = useParams<{ orderId: string }>()
  const { data: order, isPending, isError } = useOrder(orderId)
  const { data: shipment } = useShipment(orderId)

  if (isPending) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (isError || !order) {
    return <p className="text-sm text-destructive">Sipariş yüklenemedi. Lütfen sayfayı yenileyin.</p>
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Sipariş #{order.orderNumber}</h1>
          <Badge variant={order.status === 'Cancelled' ? 'destructive' : 'secondary'} className="mt-1">
            {orderStatusLabel(order.status)}
          </Badge>
        </div>
        <OrderStatusActions order={order} />
      </div>

      <section className="space-y-1 text-sm text-muted-foreground">
        <h2 className="text-base font-semibold text-foreground">Teslimat Adresi</h2>
        <p>
          {order.shippingRecipientName} · {order.shippingPhoneNumber}
        </p>
        <p>
          {order.shippingDistrict}, {order.shippingCity}
        </p>
        <p>
          {order.shippingFullAddressLine} — {order.shippingPostalCode}
        </p>
      </section>

      <section className="space-y-3">
        <h2 className="text-base font-semibold">Sipariş Özeti</h2>
        <OrderItemsList
          items={order.items.map((item) => ({
            key: item.productVariantId,
            name: item.productName,
            sku: item.sku,
            quantity: item.quantity,
            unitPrice: item.unitPrice,
            lineTotal: item.lineTotal,
          }))}
          total={order.total}
        />
      </section>

      <Separator />

      <section className="space-y-3">
        <h2 className="text-base font-semibold">Durum Geçmişi</h2>
        <OrderStatusHistoryTimeline entries={order.statusHistory} />
      </section>

      <Separator />

      <section className="space-y-3">
        <h2 className="text-base font-semibold">Kargo Takibi</h2>
        <ShipmentInfo shipment={shipment} />
        {shipment && <ShipmentActions shipment={shipment} orderId={order.id} />}
      </section>
    </div>
  )
}
