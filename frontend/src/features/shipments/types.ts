export type ShipmentStatus = 'Shipped' | 'InTransit' | 'Delivered' | 'Failed'

export type ShipmentStatusHistoryEntry = {
  status: ShipmentStatus
  note: string | null
  changedAtUtc: string
}

export type Shipment = {
  id: string
  orderId: string
  carrier: string
  trackingNumber: string
  status: ShipmentStatus
  failureReason: string | null
  createdAtUtc: string
  updatedAtUtc: string
  statusHistory: ShipmentStatusHistoryEntry[]
}
