'use client'

import { Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useDeleteAddress } from '@/features/addresses/api/useDeleteAddress'
import { useSetDefaultAddress } from '@/features/addresses/api/useSetDefaultAddress'
import type { Address } from '@/features/addresses/types'

import { AddressFormDialog } from './AddressFormDialog'

export function AddressCard({ address }: { address: Address }) {
  const setDefault = useSetDefaultAddress()
  const deleteAddress = useDeleteAddress()

  function handleDelete() {
    if (!window.confirm('Bu adresi silmek istediğinize emin misiniz?')) {
      return
    }
    deleteAddress.mutate(address.id, {
      onError: () => toast.error('Adres silinemedi. Lütfen tekrar deneyin.'),
    })
  }

  function handleSetDefault() {
    setDefault.mutate(address.id, {
      onError: () => toast.error('Varsayılan adres güncellenemedi. Lütfen tekrar deneyin.'),
    })
  }

  return (
    <div className="space-y-2 rounded-lg border p-4">
      <div className="flex items-center gap-2">
        <span className="font-medium">{address.title}</span>
        {address.isDefault && <Badge>Varsayılan</Badge>}
      </div>

      <p className="text-sm text-muted-foreground">
        {address.recipientName} · {address.phoneNumber}
      </p>
      <p className="text-sm text-muted-foreground">
        {address.district}, {address.city}
      </p>
      <p className="text-sm text-muted-foreground">
        {address.fullAddressLine} — {address.postalCode}
      </p>

      <div className="flex items-center gap-2 pt-1">
        <AddressFormDialog
          address={address}
          trigger={
            <Button type="button" variant="outline" size="sm">
              Düzenle
            </Button>
          }
        />

        {!address.isDefault && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleSetDefault}
            disabled={setDefault.isPending}
          >
            Varsayılan Yap
          </Button>
        )}

        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Adresi sil"
          onClick={handleDelete}
          disabled={deleteAddress.isPending}
        >
          <Trash2 className="size-4" />
        </Button>
      </div>
    </div>
  )
}
