'use client'

import { X } from 'lucide-react'
import { useState } from 'react'
import { toast } from 'sonner'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { useAssignCategoryAttribute } from '@/features/catalog/api/useAssignCategoryAttribute'
import { useCategoryAttributes } from '@/features/catalog/api/useCategoryAttributes'
import { useProductAttributes } from '@/features/catalog/api/useProductAttributes'
import { useRemoveCategoryAttribute } from '@/features/catalog/api/useRemoveCategoryAttribute'
import type { Category } from '@/features/catalog/types'

const nativeSelectClassName =
  'h-8 min-w-0 flex-1 rounded-lg border border-input bg-transparent px-2.5 py-1 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30'

export function CategoryAttributesDialog({ category, trigger }: { category: Category; trigger: React.ReactElement }) {
  const [open, setOpen] = useState(false)
  const [selectedAttributeId, setSelectedAttributeId] = useState('')

  const { data: assigned, isPending } = useCategoryAttributes(open ? category.id : undefined)
  const { data: allAttributes } = useProductAttributes()
  const assignAttribute = useAssignCategoryAttribute()
  const removeAttribute = useRemoveCategoryAttribute()

  const assignedIds = new Set((assigned ?? []).map((a) => a.id))
  const unassigned = (allAttributes ?? []).filter((a) => !assignedIds.has(a.id))

  function handleAssign() {
    if (!selectedAttributeId) return
    assignAttribute.mutate(
      { categoryId: category.id, productAttributeId: selectedAttributeId },
      {
        onSuccess: () => setSelectedAttributeId(''),
        onError: () => toast.error('Özellik atanamadı. Lütfen tekrar deneyin.'),
      },
    )
  }

  function handleRemove(productAttributeId: string) {
    removeAttribute.mutate(
      { categoryId: category.id, productAttributeId },
      { onError: () => toast.error('Özellik kaldırılamadı. Lütfen tekrar deneyin.') },
    )
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{category.name} — Özellikler</DialogTitle>
        </DialogHeader>

        <div className="space-y-3">
          {isPending && <p className="text-sm text-muted-foreground">Yükleniyor...</p>}

          {!isPending && assigned?.length === 0 && (
            <p className="text-sm text-muted-foreground">Bu kategoriye henüz özellik atanmadı.</p>
          )}

          {assigned && assigned.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {assigned.map((attribute) => (
                <Badge key={attribute.id} variant="secondary" className="gap-1">
                  {attribute.name}
                  <button
                    type="button"
                    aria-label={`${attribute.name} özelliğini kaldır`}
                    onClick={() => handleRemove(attribute.id)}
                    disabled={removeAttribute.isPending}
                  >
                    <X className="size-3" />
                  </button>
                </Badge>
              ))}
            </div>
          )}

          <div className="flex items-center gap-2 pt-2">
            <select
              className={nativeSelectClassName}
              value={selectedAttributeId}
              onChange={(event) => setSelectedAttributeId(event.target.value)}
            >
              <option value="">Özellik seç...</option>
              {unassigned.map((attribute) => (
                <option key={attribute.id} value={attribute.id}>
                  {attribute.name}
                </option>
              ))}
            </select>
            <Button
              type="button"
              size="sm"
              disabled={!selectedAttributeId || assignAttribute.isPending}
              onClick={handleAssign}
            >
              Ekle
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
