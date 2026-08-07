'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { AddressFormDialog } from '@/components/addresses/AddressFormDialog'
import { AddressSelector } from '@/components/checkout/AddressSelector'
import { OrderItemsList } from '@/components/checkout/OrderItemsList'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { useAddresses } from '@/features/addresses/api/useAddresses'
import { useAuthStore } from '@/features/auth/store'
import { useCart } from '@/features/cart/api/useCart'
import { getApiErrorMessage } from '@/lib/errors'
import { useCreateOrder } from '@/features/orders/api/useCreateOrder'

export default function CheckoutPage() {
  const router = useRouter()
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const { data: cart, isPending: isCartPending } = useCart()
  const { data: addresses, isPending: isAddressesPending } = useAddresses(isAuthenticated)
  const createOrder = useCreateOrder()

  const [chosenAddressId, setChosenAddressId] = useState<string | null>(null)
  const [couponCode, setCouponCode] = useState('')

  // Derived during render rather than synced via an effect: falls back to the default (or
  // first) address until the user explicitly picks a different one.
  const selectedAddressId =
    chosenAddressId ?? addresses?.find((address) => address.isDefault)?.id ?? addresses?.[0]?.id ?? null

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-3xl space-y-4 px-4 py-16 text-center">
        <p className="text-muted-foreground">Bu sayfayı görüntülemek için giriş yapmalısınız.</p>
        <Button render={<Link href="/login" />} nativeButton={false}>
          Giriş Yap
        </Button>
      </div>
    )
  }

  if (isCartPending || isAddressesPending) {
    return (
      <div className="mx-auto max-w-3xl space-y-4 px-4 py-8">
        <Skeleton className="h-24 w-full" />
        <Skeleton className="h-24 w-full" />
      </div>
    )
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="mx-auto max-w-3xl space-y-4 px-4 py-16 text-center">
        <p className="text-muted-foreground">Sepetiniz boş.</p>
        <Button render={<Link href="/" />} nativeButton={false}>
          Alışverişe başla
        </Button>
      </div>
    )
  }

  function handleSubmit() {
    if (!selectedAddressId) return
    createOrder.mutate(
      { addressId: selectedAddressId, couponCode: couponCode.trim() || undefined },
      { onSuccess: (order) => router.push(`/checkout/${order.id}`) },
    )
  }

  return (
    <div className="mx-auto max-w-3xl space-y-8 px-4 py-8">
      <h1 className="text-2xl font-bold tracking-tight">Siparişi Tamamla</h1>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Teslimat Adresi</h2>
          <AddressFormDialog
            trigger={
              <Button type="button" variant="outline" size="sm">
                Yeni Adres Ekle
              </Button>
            }
          />
        </div>

        {addresses && addresses.length > 0 ? (
          <AddressSelector
            addresses={addresses}
            selectedId={selectedAddressId}
            onSelect={setChosenAddressId}
          />
        ) : (
          <p className="text-sm text-muted-foreground">
            Henüz kayıtlı bir adresiniz yok, devam etmek için bir adres ekleyin.
          </p>
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Sipariş Özeti</h2>
        <OrderItemsList
          items={cart.items.map((item) => ({
            key: item.productVariantId,
            name: item.productName,
            sku: item.sku,
            quantity: item.quantity,
            unitPrice: item.unitPrice,
            lineTotal: item.lineTotal,
          }))}
          total={cart.total}
        />
      </section>

      <section className="max-w-sm space-y-1.5">
        <Label htmlFor="coupon-code">Kupon Kodu (opsiyonel)</Label>
        <Input
          id="coupon-code"
          value={couponCode}
          onChange={(event) => setCouponCode(event.target.value)}
        />
      </section>

      {createOrder.isError && (
        <p className="text-sm text-destructive">
          {getApiErrorMessage(createOrder.error, 'Sipariş oluşturulamadı. Lütfen tekrar deneyin.')}
        </p>
      )}

      <Button
        type="button"
        className="w-full max-w-sm"
        disabled={!selectedAddressId || createOrder.isPending}
        onClick={handleSubmit}
      >
        {createOrder.isPending ? 'Sipariş oluşturuluyor...' : 'Sipariş Ver'}
      </Button>
    </div>
  )
}
