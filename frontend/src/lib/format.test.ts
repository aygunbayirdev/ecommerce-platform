import { describe, expect, it } from 'vitest'

import { formatPrice } from './format'

describe('formatPrice', () => {
  it('formats a whole number as Turkish lira with two decimals', () => {
    expect(formatPrice(1499)).toBe('₺1.499,00')
  })

  it('formats a decimal amount using a comma as the decimal separator', () => {
    expect(formatPrice(129.5)).toBe('₺129,50')
  })

  it('uses a dot as the thousands separator', () => {
    expect(formatPrice(25000.5)).toBe('₺25.000,50')
  })
})
