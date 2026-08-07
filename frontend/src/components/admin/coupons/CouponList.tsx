'use client'

import { toast } from 'sonner'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useDeactivateCoupon } from '@/features/coupons/api/useDeactivateCoupon'
import { useReactivateCoupon } from '@/features/coupons/api/useReactivateCoupon'
import type { Coupon } from '@/features/coupons/types'
import { formatPrice } from '@/lib/format'

function discountLabel(coupon: Coupon): string {
  return coupon.discountType === 0 ? `%${coupon.discountValue}` : formatPrice(coupon.discountValue)
}

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('tr-TR', { year: 'numeric', month: 'short', day: 'numeric' })
}

export function CouponList({ coupons }: { coupons: Coupon[] }) {
  const deactivateCoupon = useDeactivateCoupon()
  const reactivateCoupon = useReactivateCoupon()

  function handleToggle(coupon: Coupon) {
    const mutation = coupon.isActive ? deactivateCoupon : reactivateCoupon
    mutation.mutate(coupon.id, {
      onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'),
    })
  }

  return (
    <div className="space-y-2">
      {coupons.map((coupon) => (
        <div key={coupon.id} className="flex items-center justify-between gap-3 rounded-lg border p-3">
          <div>
            <p className="font-medium">
              {coupon.code} — {discountLabel(coupon)}
            </p>
            <p className="text-sm text-muted-foreground">
              {formatDate(coupon.validFrom)} – {formatDate(coupon.validTo)} · Kullanım: {coupon.usedCount}
              {coupon.usageLimit != null ? ` / ${coupon.usageLimit}` : ''}
            </p>
          </div>

          <div className="flex items-center gap-2">
            {!coupon.isActive && <Badge variant="destructive">Pasif</Badge>}
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={deactivateCoupon.isPending || reactivateCoupon.isPending}
              onClick={() => handleToggle(coupon)}
            >
              {coupon.isActive ? 'Pasife Al' : 'Aktifleştir'}
            </Button>
          </div>
        </div>
      ))}
    </div>
  )
}
