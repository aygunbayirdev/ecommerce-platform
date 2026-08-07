'use client'

import { type FormEvent, useState } from 'react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useCreateShipment } from '@/features/shipments/api/useCreateShipment'
import { getApiErrorMessage } from '@/lib/errors'

export function CreateShipmentDialog({ orderId, trigger }: { orderId: string; trigger: React.ReactElement }) {
  const [open, setOpen] = useState(false)
  const [carrier, setCarrier] = useState('')
  const [trackingNumber, setTrackingNumber] = useState('')

  const createShipment = useCreateShipment()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setCarrier('')
      setTrackingNumber('')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    createShipment.mutate(
      { orderId, carrier, trackingNumber },
      {
        onSuccess: () => {
          toast.success('Sipariş kargoya verildi.')
          setOpen(false)
        },
        onError: () => toast.error('Kargo oluşturulamadı. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Kargoya Ver</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="shipment-carrier">Kargo Firması</Label>
            <Input
              id="shipment-carrier"
              required
              maxLength={100}
              value={carrier}
              onChange={(event) => setCarrier(event.target.value)}
              placeholder="Aras Kargo, Yurtiçi Kargo, vb."
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="shipment-tracking-number">Takip Numarası</Label>
            <Input
              id="shipment-tracking-number"
              required
              maxLength={100}
              value={trackingNumber}
              onChange={(event) => setTrackingNumber(event.target.value)}
            />
          </div>

          {createShipment.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(createShipment.error, 'Kargo oluşturulamadı. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={createShipment.isPending}>
              {createShipment.isPending ? 'Kaydediliyor...' : 'Kargoya Ver'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
