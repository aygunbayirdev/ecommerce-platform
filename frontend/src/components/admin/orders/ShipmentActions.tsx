'use client'

import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { useMarkShipmentDelivered } from '@/features/shipments/api/useMarkShipmentDelivered'
import { useMarkShipmentInTransit } from '@/features/shipments/api/useMarkShipmentInTransit'
import type { Shipment } from '@/features/shipments/types'

import { MarkShipmentFailedDialog } from './MarkShipmentFailedDialog'

export function ShipmentActions({ shipment, orderId }: { shipment: Shipment; orderId: string }) {
  const markInTransit = useMarkShipmentInTransit()
  const markDelivered = useMarkShipmentDelivered()

  if (shipment.status === 'Delivered' || shipment.status === 'Failed') {
    return null
  }

  function handleMarkInTransit() {
    markInTransit.mutate(
      { shipmentId: shipment.id, orderId },
      { onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.') },
    )
  }

  function handleMarkDelivered() {
    markDelivered.mutate(
      { shipmentId: shipment.id, orderId },
      { onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.') },
    )
  }

  return (
    <div className="flex items-center gap-2">
      {shipment.status === 'Shipped' && (
        <Button type="button" variant="outline" size="sm" disabled={markInTransit.isPending} onClick={handleMarkInTransit}>
          Yolda İşaretle
        </Button>
      )}

      <Button type="button" variant="outline" size="sm" disabled={markDelivered.isPending} onClick={handleMarkDelivered}>
        Teslim Edildi İşaretle
      </Button>

      <MarkShipmentFailedDialog shipmentId={shipment.id} orderId={orderId} />
    </div>
  )
}
