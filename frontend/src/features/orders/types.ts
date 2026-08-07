export type OrderStatus =
  | 'Created'
  | 'PaymentPending'
  | 'Paid'
  | 'Preparing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'

export type OrderItem = {
  productVariantId: string
  productName: string
  sku: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export type OrderStatusHistoryEntry = {
  status: OrderStatus
  note: string | null
  changedAtUtc: string
}

export type Order = {
  id: string
  orderNumber: string
  userId: string
  status: OrderStatus
  shippingRecipientName: string
  shippingPhoneNumber: string
  shippingCity: string
  shippingDistrict: string
  shippingFullAddressLine: string
  shippingPostalCode: string
  couponCode: string | null
  discountAmount: number
  createdAtUtc: string
  items: OrderItem[]
  statusHistory: OrderStatusHistoryEntry[]
  total: number
}

export type CreateOrderRequest = {
  addressId: string
  couponCode?: string
}
