import { describe, expect, it } from 'vitest'

import { shipmentStatusLabel } from './shipmentStatus'

describe('shipmentStatusLabel', () => {
  it('translates every ShipmentStatus value to a Turkish label', () => {
    expect(shipmentStatusLabel('Shipped')).toBe('Kargoya Verildi')
    expect(shipmentStatusLabel('InTransit')).toBe('Yolda')
    expect(shipmentStatusLabel('Delivered')).toBe('Teslim Edildi')
    expect(shipmentStatusLabel('Failed')).toBe('Teslimat Başarısız')
  })
})
