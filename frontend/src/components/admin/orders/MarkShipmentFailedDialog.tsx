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
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { useMarkShipmentFailed } from '@/features/shipments/api/useMarkShipmentFailed'
import { getApiErrorMessage } from '@/lib/errors'

export function MarkShipmentFailedDialog({ shipmentId, orderId }: { shipmentId: string; orderId: string }) {
  const [open, setOpen] = useState(false)
  const [reason, setReason] = useState('')
  const markFailed = useMarkShipmentFailed()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) setReason('')
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    markFailed.mutate(
      { shipmentId, orderId, reason },
      {
        onSuccess: () => {
          toast.success('Kargo başarısız olarak işaretlendi.')
          setOpen(false)
        },
        onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={<Button type="button" variant="destructive" size="sm" />}>
        Başarısız İşaretle
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Kargoyu Başarısız İşaretle</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="shipment-failure-reason">Sebep</Label>
            <Textarea
              id="shipment-failure-reason"
              required
              maxLength={500}
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              placeholder="Örn. adreste kimse bulunamadı"
            />
          </div>

          {markFailed.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(markFailed.error, 'İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" variant="destructive" disabled={markFailed.isPending}>
              {markFailed.isPending ? 'Kaydediliyor...' : 'Onayla'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
