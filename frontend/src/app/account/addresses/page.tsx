'use client'

import Link from 'next/link'

import { AddressCard } from '@/components/addresses/AddressCard'
import { AddressFormDialog } from '@/components/addresses/AddressFormDialog'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useAddresses } from '@/features/addresses/api/useAddresses'
import { useAuthStore } from '@/features/auth/store'

export default function AddressesPage() {
  const isAuthenticated = useAuthStore((state) => !!state.accessToken)
  const { data: addresses, isPending, isError } = useAddresses(isAuthenticated)

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

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Adreslerim</h1>
        <AddressFormDialog trigger={<Button type="button">Yeni Adres Ekle</Button>} />
      </div>

      {isPending && (
        <div className="space-y-4">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      )}

      {isError && (
        <p className="text-sm text-destructive">Adresler yüklenemedi. Lütfen sayfayı yenileyin.</p>
      )}

      {!isPending && !isError && addresses?.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Henüz kayıtlı bir adresiniz yok.</p>
      )}

      {addresses && addresses.length > 0 && (
        <div className="space-y-3">
          {addresses.map((address) => (
            <AddressCard key={address.id} address={address} />
          ))}
        </div>
      )}
    </div>
  )
}
