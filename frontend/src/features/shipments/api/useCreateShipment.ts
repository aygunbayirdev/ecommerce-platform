import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useCreateShipment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { orderId: string; carrier: string; trackingNumber: string }) => {
      await apiClient.post('/shipments', {
        orderId: payload.orderId,
        carrier: payload.carrier,
        trackingNumber: payload.trackingNumber,
      })
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['shipment', variables.orderId] })
      queryClient.invalidateQueries({ queryKey: ['order', variables.orderId] })
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] })
    },
  })
}
