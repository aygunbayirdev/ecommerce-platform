import { getCurrentUser, useAuthStore } from '@/features/auth/store'
import type { CurrentUser } from '@/features/auth/types'

export function useCurrentUser(): CurrentUser | null {
  const accessToken = useAuthStore((state) => state.accessToken)
  return accessToken ? getCurrentUser() : null
}
