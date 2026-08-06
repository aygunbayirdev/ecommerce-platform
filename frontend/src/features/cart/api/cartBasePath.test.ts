import { describe, expect, it } from 'vitest'

import { cartBasePath } from './cartBasePath'

describe('cartBasePath', () => {
  it('returns /carts/mine for an authenticated user regardless of guestCartId', () => {
    expect(cartBasePath(true, null)).toBe('/carts/mine')
    expect(cartBasePath(true, 'guest-123')).toBe('/carts/mine')
  })

  it('returns the guest cart path when a guestCartId exists', () => {
    expect(cartBasePath(false, 'guest-123')).toBe('/carts/guest-123')
  })

  it('returns null when there is no guest cart yet and the user is not authenticated', () => {
    expect(cartBasePath(false, null)).toBeNull()
  })
})
