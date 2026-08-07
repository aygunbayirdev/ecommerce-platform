import { describe, expect, it } from 'vitest'

import { isOrderCancellable, orderStatusLabel } from './orderStatus'

describe('orderStatusLabel', () => {
  it('translates every OrderStatus value to a Turkish label', () => {
    expect(orderStatusLabel('Created')).toBe('Oluşturuldu')
    expect(orderStatusLabel('PaymentPending')).toBe('Ödeme Bekleniyor')
    expect(orderStatusLabel('Paid')).toBe('Ödendi')
    expect(orderStatusLabel('Preparing')).toBe('Hazırlanıyor')
    expect(orderStatusLabel('Shipped')).toBe('Kargoya Verildi')
    expect(orderStatusLabel('Delivered')).toBe('Teslim Edildi')
    expect(orderStatusLabel('Cancelled')).toBe('İptal Edildi')
  })
})

describe('isOrderCancellable', () => {
  it('allows cancellation from Created, PaymentPending, Paid, and Preparing', () => {
    expect(isOrderCancellable('Created')).toBe(true)
    expect(isOrderCancellable('PaymentPending')).toBe(true)
    expect(isOrderCancellable('Paid')).toBe(true)
    expect(isOrderCancellable('Preparing')).toBe(true)
  })

  it('blocks cancellation from Shipped, Delivered, and Cancelled — mirrors Order.Cancel', () => {
    expect(isOrderCancellable('Shipped')).toBe(false)
    expect(isOrderCancellable('Delivered')).toBe(false)
    expect(isOrderCancellable('Cancelled')).toBe(false)
  })
})
