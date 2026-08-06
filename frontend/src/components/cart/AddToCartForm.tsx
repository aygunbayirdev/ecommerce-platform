'use client'

import { useState } from 'react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useAddCartItem } from '@/features/cart/api/useAddCartItem'
import type { ProductVariant } from '@/features/catalog/types'
import { formatPrice } from '@/lib/format'
import { cn } from '@/lib/utils'

function variantLabel(variant: ProductVariant): string {
  if (variant.attributeValues.length === 0) {
    return variant.sku
  }
  return variant.attributeValues.map((attr) => attr.value).join(' / ')
}

export function AddToCartForm({ variants }: { variants: ProductVariant[] }) {
  const activeVariants = variants.filter((variant) => variant.isActive)
  const [selectedId, setSelectedId] = useState(activeVariants[0]?.id)
  const [quantity, setQuantity] = useState(1)
  const addItem = useAddCartItem()

  if (activeVariants.length === 0) {
    return <p className="text-sm text-muted-foreground">Bu ürün şu anda satışta değil.</p>
  }

  const selected = activeVariants.find((variant) => variant.id === selectedId) ?? activeVariants[0]

  function handleAddToCart() {
    addItem.mutate(
      { productVariantId: selected.id, quantity },
      {
        onSuccess: () => toast.success('Ürün sepete eklendi.'),
        onError: () => toast.error('Ürün sepete eklenemedi. Lütfen tekrar deneyin.'),
      },
    )
  }

  return (
    <div className="space-y-4">
      {activeVariants.length > 1 && (
        <div className="flex flex-wrap gap-2">
          {activeVariants.map((variant) => (
            <button
              key={variant.id}
              type="button"
              onClick={() => setSelectedId(variant.id)}
              className={cn(
                'rounded-lg border px-3 py-1.5 text-sm transition-colors',
                variant.id === selected.id
                  ? 'border-primary bg-primary/5 font-medium'
                  : 'border-input hover:bg-muted',
              )}
            >
              {variantLabel(variant)}
            </button>
          ))}
        </div>
      )}

      <p className="text-xl font-semibold">{formatPrice(selected.price)}</p>

      <div className="flex items-center gap-3">
        <Input
          type="number"
          min={1}
          value={quantity}
          onChange={(event) => setQuantity(Math.max(1, Number(event.target.value) || 1))}
          className="w-20"
        />
        <Button type="button" onClick={handleAddToCart} disabled={addItem.isPending}>
          {addItem.isPending ? 'Ekleniyor...' : 'Sepete Ekle'}
        </Button>
      </div>
    </div>
  )
}
