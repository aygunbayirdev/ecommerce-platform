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
import { useCreateCategory } from '@/features/catalog/api/useCreateCategory'
import { useUpdateCategory } from '@/features/catalog/api/useUpdateCategory'
import type { Category } from '@/features/catalog/types'
import { getApiErrorMessage } from '@/lib/errors'

const nativeSelectClassName =
  'h-8 w-full min-w-0 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30'

export function CategoryFormDialog({
  category,
  categories,
  trigger,
}: {
  category?: Category
  categories: Category[]
  trigger: React.ReactElement
}) {
  const isEditMode = !!category
  const [open, setOpen] = useState(false)
  const [name, setName] = useState(category?.name ?? '')
  const [parentCategoryId, setParentCategoryId] = useState(category?.parentCategoryId ?? '')
  const [displayOrder, setDisplayOrder] = useState(category?.displayOrder ?? 0)

  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()
  const mutation = isEditMode ? updateCategory : createCategory

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setName(category?.name ?? '')
      setParentCategoryId(category?.parentCategoryId ?? '')
      setDisplayOrder(category?.displayOrder ?? 0)
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const onSuccess = () => {
      toast.success(isEditMode ? 'Kategori güncellendi.' : 'Kategori eklendi.')
      setOpen(false)
    }
    const onError = () => toast.error('Kategori kaydedilemedi. Lütfen tekrar deneyin.')

    if (isEditMode) {
      updateCategory.mutate({ categoryId: category.id, name, displayOrder }, { onSuccess, onError })
    } else {
      createCategory.mutate(
        { name, parentCategoryId: parentCategoryId || null, displayOrder },
        { onSuccess, onError },
      )
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditMode ? 'Kategoriyi Düzenle' : 'Yeni Kategori Ekle'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label htmlFor="category-name">Ad</Label>
            <Input
              id="category-name"
              required
              maxLength={200}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          {!isEditMode && (
            <div className="space-y-1.5">
              <Label htmlFor="category-parent">Üst Kategori</Label>
              <select
                id="category-parent"
                className={nativeSelectClassName}
                value={parentCategoryId}
                onChange={(event) => setParentCategoryId(event.target.value)}
              >
                <option value="">(Yok — ana kategori)</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>
          )}

          <div className="space-y-1.5">
            <Label htmlFor="category-display-order">Sıra</Label>
            <Input
              id="category-display-order"
              type="number"
              required
              value={displayOrder}
              onChange={(event) => setDisplayOrder(Number(event.target.value) || 0)}
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(mutation.error, 'Kategori kaydedilemedi. Lütfen tekrar deneyin.')}
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
