'use client'

import { CircleCheck } from 'lucide-react'
import Link from 'next/link'
import { useParams } from 'next/navigation'

import { OrderItemsList } from '@/components/checkout/OrderItemsList'
import { PaymentForm } from '@/components/checkout/PaymentForm'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useOrder } from '@/features/orders/api/useOrder'
import { usePayment } from '@/features/payments/api/usePayment'
import { formatPrice } from '@/lib/format'

export default function CheckoutOrderPage() {
  const { orderId } = useParams<{ orderId: string }>()
  const { data: order, isPending: isOrderPending, isError: isOrderError } = useOrder(orderId)
  const { data: payment, refetch: refetchPayment } = usePayment(orderId)

  if (isOrderPending) {
    return (
      <div className="mx-auto max-w-xl space-y-4 px-4 py-8">
        <Skeleton className="h-24 w-full" />
      </div>
    )
  }

  if (isOrderError || !order) {
    return (
      <div className="mx-auto max-w-xl px-4 py-16 text-center text-muted-foreground">
        Sipariş bulunamadı.
      </div>
    )
  }

  const isPaid = payment?.status === 'Succeeded'

  return (
    <div className="mx-auto max-w-xl space-y-6 px-4 py-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Sipariş #{order.orderNumber}</h1>
        <p className="text-sm text-muted-foreground">{formatPrice(order.total)}</p>
      </div>

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

      {isPaid ? (
        <div className="flex flex-col items-center gap-3 rounded-lg border p-6 text-center">
          <CircleCheck className="size-10 text-primary" />
          <p className="font-medium">Ödemeniz alındı, siparişiniz hazırlanıyor.</p>
          <Button render={<Link href="/" />} nativeButton={false} variant="outline">
            Ana Sayfaya Dön
          </Button>
        </div>
      ) : payment ? (
        <PaymentForm orderId={order.id} onSuccess={() => refetchPayment()} />
      ) : (
        <div className="space-y-2">
          <p className="text-sm text-muted-foreground">Ödeme hazırlanıyor...</p>
          <Skeleton className="h-40 w-full max-w-sm" />
        </div>
      )}
    </div>
  )
}
