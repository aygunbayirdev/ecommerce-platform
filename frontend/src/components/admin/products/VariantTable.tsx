'use client'

import { toast } from 'sonner'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useDeactivateProductVariant } from '@/features/catalog/api/useDeactivateProductVariant'
import { useReactivateProductVariant } from '@/features/catalog/api/useReactivateProductVariant'
import type { ProductVariant } from '@/features/catalog/types'
import { formatPrice } from '@/lib/format'

function variantLabel(variant: ProductVariant): string {
  if (variant.attributeValues.length === 0) return variant.sku
  return variant.attributeValues.map((attr) => `${attr.productAttributeName}: ${attr.value}`).join(', ')
}

export function VariantTable({ productId, variants }: { productId: string; variants: ProductVariant[] }) {
  const deactivateVariant = useDeactivateProductVariant()
  const reactivateVariant = useReactivateProductVariant()

  function handleToggle(variant: ProductVariant) {
    const mutation = variant.isActive ? deactivateVariant : reactivateVariant
    mutation.mutate(
      { productId, variantId: variant.id },
      { onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.') },
    )
  }

  if (variants.length === 0) {
    return <p className="text-sm text-muted-foreground">Henüz varyant eklenmedi.</p>
  }

  return (
    <div className="space-y-2">
      {variants.map((variant) => (
        <div key={variant.id} className="flex items-center justify-between gap-3 rounded-lg border p-3">
          <div>
            <p className="font-medium">{variant.sku}</p>
            <p className="text-sm text-muted-foreground">{variantLabel(variant)}</p>
          </div>

          <div className="flex items-center gap-2">
            <span className="text-sm">{formatPrice(variant.price)}</span>
            {!variant.isActive && <Badge variant="destructive">Pasif</Badge>}
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={deactivateVariant.isPending || reactivateVariant.isPending}
              onClick={() => handleToggle(variant)}
            >
              {variant.isActive ? 'Pasife Al' : 'Aktifleştir'}
            </Button>
          </div>
        </div>
      ))}
    </div>
  )
}
