import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useMarkShipmentDelivered() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { shipmentId: string; orderId: string }) => {
      await apiClient.post(`/shipments/${payload.shipmentId}/mark-delivered`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['shipment', variables.orderId] })
      queryClient.invalidateQueries({ queryKey: ['order', variables.orderId] })
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] })
    },
  })
}
