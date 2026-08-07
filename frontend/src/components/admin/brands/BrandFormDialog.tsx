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
import { useCreateBrand } from '@/features/catalog/api/useCreateBrand'
import { useUpdateBrand } from '@/features/catalog/api/useUpdateBrand'
import type { Brand } from '@/features/catalog/types'
import { getApiErrorMessage } from '@/lib/errors'

export function BrandFormDialog({ brand, trigger }: { brand?: Brand; trigger: React.ReactElement }) {
  const isEditMode = !!brand
  const [open, setOpen] = useState(false)
  const [name, setName] = useState(brand?.name ?? '')

  const createBrand = useCreateBrand()
  const updateBrand = useUpdateBrand()
  const mutation = isEditMode ? updateBrand : createBrand

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setName(brand?.name ?? '')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const onSuccess = () => {
      toast.success(isEditMode ? 'Marka güncellendi.' : 'Marka eklendi.')
      setOpen(false)
    }
    const onError = () => toast.error('Marka kaydedilemedi. Lütfen tekrar deneyin.')

    if (isEditMode) {
      updateBrand.mutate({ brandId: brand.id, name }, { onSuccess, onError })
    } else {
      createBrand.mutate(name, { onSuccess, onError })
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditMode ? 'Markayı Düzenle' : 'Yeni Marka Ekle'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="brand-name">Ad</Label>
            <Input
              id="brand-name"
              required
              maxLength={200}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(mutation.error, 'Marka kaydedilemedi. Lütfen tekrar deneyin.')}
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
