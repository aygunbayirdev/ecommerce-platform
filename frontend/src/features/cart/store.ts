import { create } from 'zustand'
import { persist } from 'zustand/middleware'

// Guest carts have no cookie/session infrastructure (see backend CartsController) — the client
// stores the CartId it gets back from POST /carts and passes it on every subsequent call. Once
// the user logs in, /carts/mine is used instead and this id is simply ignored (no cart merge).
type GuestCartState = {
  guestCartId: string | null
  setGuestCartId: (id: string) => void
  clearGuestCartId: () => void
}

export const useGuestCartStore = create<GuestCartState>()(
  persist(
    (set) => ({
      guestCartId: null,
      setGuestCartId: (id) => set({ guestCartId: id }),
      clearGuestCartId: () => set({ guestCartId: null }),
    }),
    { name: 'ecommerce-guest-cart' },
  ),
)
