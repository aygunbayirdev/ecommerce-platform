import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useMarkShipmentFailed() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { shipmentId: string; orderId: string; reason: string }) => {
      await apiClient.post(`/shipments/${payload.shipmentId}/mark-failed`, { reason: payload.reason })
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['shipment', variables.orderId] })
    },
  })
}
