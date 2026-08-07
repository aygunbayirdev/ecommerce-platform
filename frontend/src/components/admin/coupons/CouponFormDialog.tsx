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
import { useCreateCoupon } from '@/features/coupons/api/useCreateCoupon'
import type { CouponDiscountType } from '@/features/coupons/types'
import { getApiErrorMessage } from '@/lib/errors'

const nativeSelectClassName =
  'h-8 w-full min-w-0 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30'

function toDateInputValue(date: Date): string {
  return date.toISOString().slice(0, 10)
}

function defaultValidToDate(): Date {
  const date = new Date()
  date.setDate(date.getDate() + 30)
  return date
}

export function CouponFormDialog({ trigger }: { trigger: React.ReactElement }) {
  const [open, setOpen] = useState(false)
  const [code, setCode] = useState('')
  const [discountType, setDiscountType] = useState<CouponDiscountType>(0)
  const [discountValue, setDiscountValue] = useState('')
  const [validFrom, setValidFrom] = useState(() => toDateInputValue(new Date()))
  const [validTo, setValidTo] = useState(() => toDateInputValue(defaultValidToDate()))
  const [usageLimit, setUsageLimit] = useState('')

  const createCoupon = useCreateCoupon()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setCode('')
      setDiscountType(0)
      setDiscountValue('')
      setValidFrom(toDateInputValue(new Date()))
      setValidTo(toDateInputValue(defaultValidToDate()))
      setUsageLimit('')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    createCoupon.mutate(
      {
        code,
        discountType,
        discountValue: Number(discountValue),
        validFrom: new Date(validFrom).toISOString(),
        validTo: new Date(validTo).toISOString(),
        usageLimit: usageLimit ? Number(usageLimit) : null,
      },
      {
        onSuccess: () => {
          toast.success('Kupon eklendi.')
          setOpen(false)
        },
        onError: () => toast.error('Kupon kaydedilemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Yeni Kupon Ekle</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="coupon-code">Kod</Label>
            <Input
              id="coupon-code"
              required
              maxLength={50}
              value={code}
              onChange={(event) => setCode(event.target.value)}
              placeholder="INDIRIM10"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label htmlFor="coupon-discount-type">İndirim Tipi</Label>
              <select
                id="coupon-discount-type"
                className={nativeSelectClassName}
                value={discountType}
                onChange={(event) => setDiscountType(Number(event.target.value) as CouponDiscountType)}
              >
                <option value={0}>Yüzde</option>
                <option value={1}>Sabit Tutar</option>
              </select>
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="coupon-discount-value">
                {discountType === 0 ? 'Yüzde (%)' : 'Tutar (₺)'}
              </Label>
              <Input
                id="coupon-discount-value"
                type="number"
                required
                min={0.01}
                step="0.01"
                value={discountValue}
                onChange={(event) => setDiscountValue(event.target.value)}
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label htmlFor="coupon-valid-from">Başlangıç</Label>
              <Input
                id="coupon-valid-from"
                type="date"
                required
                value={validFrom}
                onChange={(event) => setValidFrom(event.target.value)}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="coupon-valid-to">Bitiş</Label>
              <Input
                id="coupon-valid-to"
                type="date"
                required
                value={validTo}
                onChange={(event) => setValidTo(event.target.value)}
              />
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="coupon-usage-limit">Kullanım Limiti (opsiyonel)</Label>
            <Input
              id="coupon-usage-limit"
              type="number"
              min={1}
              value={usageLimit}
              onChange={(event) => setUsageLimit(event.target.value)}
              placeholder="Sınırsız"
            />
          </div>

          {createCoupon.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(createCoupon.error, 'Kupon kaydedilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={createCoupon.isPending}>
              {createCoupon.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
