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
import { useAddProductVariant } from '@/features/catalog/api/useAddProductVariant'
import { useCategoryAttributes } from '@/features/catalog/api/useCategoryAttributes'
import { getApiErrorMessage } from '@/lib/errors'

export function AddVariantDialog({
  productId,
  categoryId,
  trigger,
}: {
  productId: string
  categoryId: string
  trigger: React.ReactElement
}) {
  const [open, setOpen] = useState(false)
  const [sku, setSku] = useState('')
  const [price, setPrice] = useState('')
  const [attributeValues, setAttributeValues] = useState<Record<string, string>>({})

  const { data: categoryAttributes } = useCategoryAttributes(open ? categoryId : undefined)
  const addVariant = useAddProductVariant()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setSku('')
      setPrice('')
      setAttributeValues({})
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const values = Object.entries(attributeValues)
      .filter(([, value]) => value.trim() !== '')
      .map(([productAttributeId, value]) => ({ productAttributeId, value }))

    addVariant.mutate(
      { productId, sku, price: Number(price), attributeValues: values },
      {
        onSuccess: () => {
          toast.success('Varyant eklendi.')
          setOpen(false)
        },
        onError: () => toast.error('Varyant kaydedilemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Yeni Varyant Ekle</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="variant-sku">SKU</Label>
            <Input id="variant-sku" required maxLength={64} value={sku} onChange={(event) => setSku(event.target.value)} />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="variant-price">Fiyat</Label>
            <Input
              id="variant-price"
              type="number"
              required
              min={0.01}
              step="0.01"
              value={price}
              onChange={(event) => setPrice(event.target.value)}
            />
          </div>

          {categoryAttributes && categoryAttributes.length === 0 && (
            <p className="text-sm text-muted-foreground">
              Bu kategoriye henüz özellik atanmadı — Kategoriler sayfasından özellik atayabilirsiniz.
            </p>
          )}

          {categoryAttributes?.map((attribute) => (
            <div key={attribute.id} className="space-y-1.5">
              <Label htmlFor={`variant-attr-${attribute.id}`}>{attribute.name}</Label>
              <Input
                id={`variant-attr-${attribute.id}`}
                maxLength={200}
                value={attributeValues[attribute.id] ?? ''}
                onChange={(event) =>
                  setAttributeValues((prev) => ({ ...prev, [attribute.id]: event.target.value }))
                }
              />
            </div>
          ))}

          {addVariant.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(addVariant.error, 'Varyant kaydedilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={addVariant.isPending}>
              {addVariant.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
