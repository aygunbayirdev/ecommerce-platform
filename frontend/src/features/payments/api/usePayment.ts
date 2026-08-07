import { useQuery } from '@tanstack/react-query'

import { paymentApiClient } from '@/lib/axios'

import type { Payment } from '../types'

// The Payment record for an order is created asynchronously (Order publishes an integration
// event over RabbitMQ, payment-service consumes it) — there's a real window right after checkout
// where no Payment row exists yet. Poll until it shows up instead of racing a single request.
export function usePayment(orderId: string) {
  return useQuery({
    queryKey: ['payment', orderId],
    queryFn: async () => {
      const response = await paymentApiClient.get<Payment>(`/payments/by-order/${orderId}`)
      return response.data
    },
    retry: false,
    refetchInterval: (query) => (query.state.data ? false : 2000),
    // The user is actively waiting on this — don't stop polling just because they alt-tab
    // while the async event is still in flight.
    refetchIntervalInBackground: true,
  })
}
