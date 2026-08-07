import type { ShipmentStatus } from '@/features/shipments/types'

const labels: Record<ShipmentStatus, string> = {
  Shipped: 'Kargoya Verildi',
  InTransit: 'Yolda',
  Delivered: 'Teslim Edildi',
  Failed: 'Teslimat Başarısız',
}

export function shipmentStatusLabel(status: ShipmentStatus): string {
  return labels[status]
}
