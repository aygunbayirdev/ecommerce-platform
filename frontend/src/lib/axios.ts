import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'

import { useAuthStore } from '@/features/auth/store'
import type { AuthTokens } from '@/features/auth/types'

const monolithBaseUrl = process.env.NEXT_PUBLIC_API_URL
const paymentBaseUrl = process.env.NEXT_PUBLIC_PAYMENT_API_URL

type RetriableRequestConfig = InternalAxiosRequestConfig & { _retry?: boolean }

let refreshPromise: Promise<AuthTokens> | null = null

async function refreshAccessToken(): Promise<AuthTokens> {
  const { refreshToken } = useAuthStore.getState()
  if (!refreshToken) {
    throw new Error('No refresh token available')
  }

  // Tokens are only ever issued/refreshed by the monolith's Identity module —
  // payment-service validates the same JWT locally but has no /auth/refresh of its own.
  const response = await axios.post<AuthTokens>(`${monolithBaseUrl}/auth/refresh`, { refreshToken })
  return response.data
}

function redirectToLogin() {
  useAuthStore.getState().clear()
  if (typeof window !== 'undefined') {
    window.location.assign('/login')
  }
}

function createApiClient(baseURL: string | undefined): AxiosInstance {
  const client = axios.create({ baseURL })

  client.interceptors.request.use((config) => {
    const { accessToken } = useAuthStore.getState()
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`
    }
    return config
  })

  client.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as RetriableRequestConfig | undefined
      const status = error.response?.status
      const isRefreshCall = originalRequest?.url?.includes('/auth/refresh')
      const isLoginCall = originalRequest?.url?.includes('/auth/login')

      if (status !== 401 || !originalRequest || isLoginCall) {
        return Promise.reject(error)
      }

      if (isRefreshCall || originalRequest._retry) {
        redirectToLogin()
        return Promise.reject(error)
      }

      originalRequest._retry = true

      try {
        refreshPromise ??= refreshAccessToken()
        const tokens = await refreshPromise
        useAuthStore.getState().setTokens(tokens)
        originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`
        return client(originalRequest)
      } catch (refreshError) {
        redirectToLogin()
        return Promise.reject(refreshError)
      } finally {
        refreshPromise = null
      }
    },
  )

  return client
}

// Faz 4: Order/Cart/Identity/Catalog/etc. live in the monolith; Payment lives in its own
// service (see CLAUDE.md madde 10) with its own origin — the client talks to it directly,
// never through the monolith.
export const apiClient = createApiClient(monolithBaseUrl)
export const paymentApiClient = createApiClient(paymentBaseUrl)
