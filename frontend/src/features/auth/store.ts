import { create } from 'zustand'
import { persist } from 'zustand/middleware'

import { decodeJwtPayload } from '@/lib/jwt'

import type { AuthTokens, CurrentUser } from './types'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

type JwtPayload = {
  sub: string
  email: string
  [ROLE_CLAIM]: 'Customer' | 'Admin'
}

type AuthState = {
  accessToken: string | null
  refreshToken: string | null
  setTokens: (tokens: AuthTokens) => void
  clear: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      setTokens: (tokens) =>
        set({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
        }),
      clear: () => set({ accessToken: null, refreshToken: null }),
    }),
    { name: 'ecommerce-auth' },
  ),
)

export function getCurrentUser(): CurrentUser | null {
  const { accessToken } = useAuthStore.getState()
  if (!accessToken) return null

  const payload = decodeJwtPayload<JwtPayload>(accessToken)
  if (!payload) return null

  return { userId: payload.sub, email: payload.email, role: payload[ROLE_CLAIM] }
}
