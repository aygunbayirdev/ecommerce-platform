import type { OrderStatus } from '@/features/orders/types'

const labels: Record<OrderStatus, string> = {
  Created: 'Oluşturuldu',
  PaymentPending: 'Ödeme Bekleniyor',
  Paid: 'Ödendi',
  Preparing: 'Hazırlanıyor',
  Shipped: 'Kargoya Verildi',
  Delivered: 'Teslim Edildi',
  Cancelled: 'İptal Edildi',
}

export function orderStatusLabel(status: OrderStatus): string {
  return labels[status]
}

// Mirrors Order.Cancel's domain guard exactly (backend/.../Order.Domain/Order.cs) — only show a
// cancel action when the request would actually succeed.
const cancellableStatuses: OrderStatus[] = ['Created', 'PaymentPending', 'Paid', 'Preparing']

export function isOrderCancellable(status: OrderStatus): boolean {
  return cancellableStatuses.includes(status)
}
