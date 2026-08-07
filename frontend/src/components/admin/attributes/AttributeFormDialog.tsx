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
import { useCreateProductAttribute } from '@/features/catalog/api/useCreateProductAttribute'
import { getApiErrorMessage } from '@/lib/errors'

export function AttributeFormDialog({ trigger }: { trigger: React.ReactElement }) {
  const [open, setOpen] = useState(false)
  const [name, setName] = useState('')

  const createAttribute = useCreateProductAttribute()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setName('')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    createAttribute.mutate(name, {
      onSuccess: () => {
        toast.success('Özellik eklendi.')
        setOpen(false)
      },
      onError: () => toast.error('Özellik kaydedilemedi. Lütfen tekrar deneyin.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Yeni Özellik Ekle</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="attribute-name">Ad</Label>
            <Input
              id="attribute-name"
              required
              maxLength={100}
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="Renk, Beden, vb."
            />
          </div>

          {createAttribute.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(createAttribute.error, 'Özellik kaydedilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={createAttribute.isPending}>
              {createAttribute.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
