import { useMutation, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

export function useMarkShipmentInTransit() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: { shipmentId: string; orderId: string }) => {
      await apiClient.post(`/shipments/${payload.shipmentId}/mark-in-transit`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['shipment', variables.orderId] })
    },
  })
}
