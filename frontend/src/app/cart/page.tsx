'use client'

import Link from 'next/link'

import { Button } from '@/components/ui/button'
import { CartItemRow } from '@/components/cart/CartItemRow'
import { Skeleton } from '@/components/ui/skeleton'
import { useCart } from '@/features/cart/api/useCart'
import { formatPrice } from '@/lib/format'

export default function CartPage() {
  const { data: cart, isPending, isError } = useCart()

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold tracking-tight">Sepetim</h1>

      {isPending && (
        <div className="space-y-4">
          <Skeleton className="h-20 w-full" />
          <Skeleton className="h-20 w-full" />
        </div>
      )}

      {isError && (
        <p className="text-sm text-destructive">Sepet yüklenemedi. Lütfen sayfayı yenileyin.</p>
      )}

      {!isPending && !isError && (!cart || cart.items.length === 0) && (
        <div className="space-y-4 py-16 text-center">
          <p className="text-muted-foreground">Sepetiniz boş.</p>
          <Button render={<Link href="/" />} nativeButton={false}>
            Alışverişe başla
          </Button>
        </div>
      )}

      {cart && cart.items.length > 0 && (
        <>
          <ul className="divide-y">
            {cart.items.map((item) => (
              <CartItemRow key={item.productVariantId} item={item} />
            ))}
          </ul>

          <div className="mt-6 flex items-center justify-between border-t pt-4">
            <span className="text-lg font-semibold">Toplam</span>
            <span className="text-lg font-semibold">{formatPrice(cart.total)}</span>
          </div>
        </>
      )}
    </div>
  )
}
