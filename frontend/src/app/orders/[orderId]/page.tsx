'use client'

import Link from 'next/link'
import { useParams } from 'next/navigation'

import { CancelOrderDialog } from '@/components/orders/CancelOrderDialog'
import { OrderStatusHistoryTimeline } from '@/components/orders/OrderStatusHistoryTimeline'
import { ShipmentInfo } from '@/components/orders/ShipmentInfo'
import { OrderItemsList } from '@/components/checkout/OrderItemsList'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { useAuthStore } from '@/features/auth/store'
import { useOrder } from '@/features/orders/api/useOrder'
import { useShipment } from '@/features/shipments/api/useShipment'
import { isOrderCancellable, orderStatusLabel } from '@/lib/orderStatus'

export default function OrderDetailPage() {
  const { orderId } = useParams<{ orderId: string }>()
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const { data: order, isPending, isError } = useOrder(orderId, isAuthenticated)
  const { data: shipment } = useShipment(orderId, isAuthenticated)

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

  if (isPending) {
    return (
      <div className="mx-auto max-w-3xl space-y-4 px-4 py-8">
        <Skeleton className="h-24 w-full" />
      </div>
    )
  }

  if (isError || !order) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center text-muted-foreground">
        Sipariş bulunamadı.
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl space-y-8 px-4 py-8">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Sipariş #{order.orderNumber}</h1>
          <Badge variant={order.status === 'Cancelled' ? 'destructive' : 'secondary'} className="mt-1">
            {orderStatusLabel(order.status)}
          </Badge>
        </div>
        {isOrderCancellable(order.status) && <CancelOrderDialog orderId={order.id} />}
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
      </section>
    </div>
  )
}
