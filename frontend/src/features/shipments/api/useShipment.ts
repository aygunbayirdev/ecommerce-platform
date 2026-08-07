import { useQuery } from '@tanstack/react-query'
import { AxiosError } from 'axios'

import { apiClient } from '@/lib/axios'

import type { Shipment } from '../types'

// A 404 here just means the order hasn't shipped yet — a normal state, not an error — so it's
// resolved to `null` data rather than left as a query error for the UI to special-case.
export function useShipment(orderId: string, enabled: boolean = true) {
  return useQuery({
    queryKey: ['shipment', orderId],
    queryFn: async () => {
      try {
        const response = await apiClient.get<Shipment>(`/shipments/by-order/${orderId}`)
        return response.data
      } catch (error) {
        if (error instanceof AxiosError && error.response?.status === 404) {
          return null
        }
        throw error
      }
    },
    enabled,
  })
}
