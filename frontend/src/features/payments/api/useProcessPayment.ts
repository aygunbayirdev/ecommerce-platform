import { useMutation, useQueryClient } from '@tanstack/react-query'

import { paymentApiClient } from '@/lib/axios'

export function useProcessPayment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { orderId: string; cardNumber: string }) => {
      await paymentApiClient.post('/payments', {
        orderId: payload.orderId,
        cardNumber: payload.cardNumber,
        // A fresh key per attempt — reusing one (e.g. after a decline) is rejected as a
        // duplicate rather than treated as a new charge attempt (see Payment.Attempt).
        idempotencyKey: crypto.randomUUID(),
      })
    },
    onSuccess: (_data, variables) =>
      queryClient.invalidateQueries({ queryKey: ['payment', variables.orderId] }),
  })
}
