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
import { useAdminCancelOrder } from '@/features/orders/api/useAdminCancelOrder'
import { getApiErrorMessage } from '@/lib/errors'

export function AdminCancelOrderDialog({ orderId }: { orderId: string }) {
  const [open, setOpen] = useState(false)
  const [reason, setReason] = useState('')
  const cancelOrder = useAdminCancelOrder()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) setReason('')
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    cancelOrder.mutate(
      { orderId, reason },
      {
        onSuccess: () => {
          toast.success('Sipariş iptal edildi.')
          setOpen(false)
        },
        onError: () => toast.error('Sipariş iptal edilemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={<Button type="button" variant="destructive" size="sm" />}>
        Siparişi İptal Et
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Siparişi İptal Et</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="admin-cancel-reason">İptal Sebebi</Label>
            <Textarea
              id="admin-cancel-reason"
              required
              maxLength={500}
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              placeholder="Örn. müşteri telefonla iptal istedi"
            />
          </div>

          {cancelOrder.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(cancelOrder.error, 'Sipariş iptal edilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" variant="destructive" disabled={cancelOrder.isPending}>
              {cancelOrder.isPending ? 'İptal ediliyor...' : 'İptali Onayla'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
