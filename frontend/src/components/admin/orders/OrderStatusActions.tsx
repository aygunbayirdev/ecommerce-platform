'use client'

import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { useMarkOrderAsPaid } from '@/features/orders/api/useMarkOrderAsPaid'
import { useMarkOrderAsPreparing } from '@/features/orders/api/useMarkOrderAsPreparing'
import type { Order } from '@/features/orders/types'
import { isOrderCancellable } from '@/lib/orderStatus'

import { AdminCancelOrderDialog } from './AdminCancelOrderDialog'
import { CreateShipmentDialog } from './CreateShipmentDialog'

export function OrderStatusActions({ order }: { order: Order }) {
  const markAsPaid = useMarkOrderAsPaid()
  const markAsPreparing = useMarkOrderAsPreparing()

  function handleMarkAsPaid() {
    markAsPaid.mutate(order.id, {
      onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'),
    })
  }

  function handleMarkAsPreparing() {
    markAsPreparing.mutate(order.id, {
      onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'),
    })
  }

  return (
    <div className="flex flex-wrap items-center gap-2">
      {order.status === 'PaymentPending' && (
        <Button type="button" disabled={markAsPaid.isPending} onClick={handleMarkAsPaid}>
          Ödendi İşaretle
        </Button>
      )}

      {order.status === 'Paid' && (
        <Button type="button" disabled={markAsPreparing.isPending} onClick={handleMarkAsPreparing}>
          Hazırlanıyor İşaretle
        </Button>
      )}

      {order.status === 'Preparing' && (
        <CreateShipmentDialog orderId={order.id} trigger={<Button type="button">Kargoya Ver</Button>} />
      )}

      {isOrderCancellable(order.status) && <AdminCancelOrderDialog orderId={order.id} />}
    </div>
  )
}
