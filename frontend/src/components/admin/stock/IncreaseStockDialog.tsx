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
import { useIncreaseStock } from '@/features/inventory/api/useIncreaseStock'
import { getApiErrorMessage } from '@/lib/errors'

export function IncreaseStockDialog({
  productVariantId,
  productName,
  trigger,
}: {
  productVariantId: string
  productName: string
  trigger: React.ReactElement
}) {
  const [open, setOpen] = useState(false)
  const [quantity, setQuantity] = useState(1)
  const [reason, setReason] = useState('')

  const increaseStock = useIncreaseStock()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setQuantity(1)
      setReason('')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    increaseStock.mutate(
      { productVariantId, quantity, reason: reason || undefined },
      {
        onSuccess: () => {
          toast.success('Stok artırıldı.')
          setOpen(false)
        },
        onError: () => toast.error('Stok artırılamadı. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Stok Artır — {productName}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="stock-quantity">Miktar</Label>
            <Input
              id="stock-quantity"
              type="number"
              required
              min={1}
              value={quantity}
              onChange={(event) => setQuantity(Math.max(1, Number(event.target.value) || 1))}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="stock-reason">Açıklama (opsiyonel)</Label>
            <Input id="stock-reason" maxLength={200} value={reason} onChange={(event) => setReason(event.target.value)} />
          </div>

          {increaseStock.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(increaseStock.error, 'Stok artırılamadı. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={increaseStock.isPending}>
              {increaseStock.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
