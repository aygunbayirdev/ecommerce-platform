export function cartBasePath(isAuthenticated: boolean, guestCartId: string | null): string | null {
  if (isAuthenticated) return '/carts/mine'
  return guestCartId ? `/carts/${guestCartId}` : null
}
