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
import { useAddAddress } from '@/features/addresses/api/useAddAddress'
import { useUpdateAddress } from '@/features/addresses/api/useUpdateAddress'
import type { Address, AddressFormValues } from '@/features/addresses/types'
import { getApiErrorMessage } from '@/lib/errors'

const emptyValues: AddressFormValues = {
  title: '',
  recipientName: '',
  phoneNumber: '',
  city: '',
  district: '',
  fullAddressLine: '',
  postalCode: '',
}

export function AddressFormDialog({ address, trigger }: { address?: Address; trigger: React.ReactElement }) {
  const isEditMode = !!address
  const [open, setOpen] = useState(false)
  const [values, setValues] = useState<AddressFormValues>(address ?? emptyValues)

  const addAddress = useAddAddress()
  const updateAddress = useUpdateAddress()
  const mutation = isEditMode ? updateAddress : addAddress

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setValues(address ?? emptyValues)
    }
  }

  function handleChange(field: keyof AddressFormValues) {
    return (event: React.ChangeEvent<HTMLInputElement>) => {
      setValues((prev) => ({ ...prev, [field]: event.target.value }))
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const onSuccess = () => {
      toast.success(isEditMode ? 'Adres güncellendi.' : 'Adres eklendi.')
      setOpen(false)
    }
    const onError = () => toast.error('Adres kaydedilemedi. Lütfen tekrar deneyin.')

    if (isEditMode) {
      updateAddress.mutate({ addressId: address.id, values }, { onSuccess, onError })
    } else {
      addAddress.mutate(values, { onSuccess, onError })
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditMode ? 'Adresi Düzenle' : 'Yeni Adres Ekle'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="address-title">Başlık</Label>
            <Input
              id="address-title"
              required
              maxLength={100}
              value={values.title}
              onChange={handleChange('title')}
              placeholder="Ev, İş, vb."
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="address-recipient">Alıcı Adı Soyadı</Label>
            <Input
              id="address-recipient"
              required
              maxLength={200}
              value={values.recipientName}
              onChange={handleChange('recipientName')}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="address-phone">Telefon</Label>
            <Input
              id="address-phone"
              type="tel"
              required
              maxLength={20}
              value={values.phoneNumber}
              onChange={handleChange('phoneNumber')}
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label htmlFor="address-city">İl</Label>
              <Input
                id="address-city"
                required
                maxLength={100}
                value={values.city}
                onChange={handleChange('city')}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="address-district">İlçe</Label>
              <Input
                id="address-district"
                required
                maxLength={100}
                value={values.district}
                onChange={handleChange('district')}
              />
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="address-line">Açık Adres</Label>
            <Input
              id="address-line"
              required
              maxLength={500}
              value={values.fullAddressLine}
              onChange={handleChange('fullAddressLine')}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="address-postal-code">Posta Kodu</Label>
            <Input
              id="address-postal-code"
              required
              maxLength={10}
              value={values.postalCode}
              onChange={handleChange('postalCode')}
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(mutation.error, 'Adres kaydedilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
