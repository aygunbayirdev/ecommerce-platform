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
import { useAddProductImage } from '@/features/catalog/api/useAddProductImage'
import { getApiErrorMessage } from '@/lib/errors'

export function AddImageDialog({ productId, trigger }: { productId: string; trigger: React.ReactElement }) {
  const [open, setOpen] = useState(false)
  const [url, setUrl] = useState('')
  const [isPrimary, setIsPrimary] = useState(false)

  const addImage = useAddProductImage()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setUrl('')
      setIsPrimary(false)
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    addImage.mutate(
      { productId, url, isPrimary },
      {
        onSuccess: () => {
          toast.success('Görsel eklendi.')
          setOpen(false)
        },
        onError: () => toast.error('Görsel kaydedilemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Yeni Görsel Ekle</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="image-url">Görsel URL</Label>
            <Input
              id="image-url"
              type="url"
              required
              value={url}
              onChange={(event) => setUrl(event.target.value)}
              placeholder="https://..."
            />
          </div>

          <label className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={isPrimary}
              onChange={(event) => setIsPrimary(event.target.checked)}
            />
            Birincil görsel yap
          </label>

          {addImage.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(addImage.error, 'Görsel kaydedilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={addImage.isPending}>
              {addImage.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
