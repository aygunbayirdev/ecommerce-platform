'use client'

import { useRouter } from 'next/navigation'
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
import { Textarea } from '@/components/ui/textarea'
import { useBrands } from '@/features/catalog/api/useBrands'
import { useCreateProduct } from '@/features/catalog/api/useCreateProduct'
import type { Category } from '@/features/catalog/types'
import { getApiErrorMessage } from '@/lib/errors'

const nativeSelectClassName =
  'h-8 w-full min-w-0 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30'

export function ProductFormDialog({ categories, trigger }: { categories: Category[]; trigger: React.ReactElement }) {
  const router = useRouter()
  const [open, setOpen] = useState(false)
  const [categoryId, setCategoryId] = useState('')
  const [brandId, setBrandId] = useState('')
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')

  const { data: brands } = useBrands()
  const createProduct = useCreateProduct()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setCategoryId('')
      setBrandId('')
      setName('')
      setDescription('')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    createProduct.mutate(
      { categoryId, brandId: brandId || null, name, description },
      {
        onSuccess: (productId) => {
          toast.success('Ürün eklendi.')
          setOpen(false)
          router.push(`/admin/products/${productId}`)
        },
        onError: () => toast.error('Ürün kaydedilemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Yeni Ürün Ekle</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="product-category">Kategori</Label>
            <select
              id="product-category"
              required
              className={nativeSelectClassName}
              value={categoryId}
              onChange={(event) => setCategoryId(event.target.value)}
            >
              <option value="" disabled>
                Kategori seçin
              </option>
              {categories.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-brand">Marka (opsiyonel)</Label>
            <select
              id="product-brand"
              className={nativeSelectClassName}
              value={brandId}
              onChange={(event) => setBrandId(event.target.value)}
            >
              <option value="">(Yok)</option>
              {(brands ?? []).map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.name}
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-name">Ad</Label>
            <Input
              id="product-name"
              required
              maxLength={200}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="product-description">Açıklama</Label>
            <Textarea
              id="product-description"
              required
              value={description}
              onChange={(event) => setDescription(event.target.value)}
            />
          </div>

          {createProduct.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(createProduct.error, 'Ürün kaydedilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={createProduct.isPending}>
              {createProduct.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
