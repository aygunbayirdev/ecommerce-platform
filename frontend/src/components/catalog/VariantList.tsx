import type { ProductVariant } from '@/features/catalog/types'
import { formatPrice } from '@/lib/format'

function variantLabel(variant: ProductVariant): string {
  if (variant.attributeValues.length === 0) {
    return variant.sku
  }
  return variant.attributeValues.map((attr) => `${attr.productAttributeName}: ${attr.value}`).join(', ')
}

export function VariantList({ variants }: { variants: ProductVariant[] }) {
  const activeVariants = variants.filter((variant) => variant.isActive)

  if (activeVariants.length === 0) {
    return <p className="text-sm text-muted-foreground">Bu ürün şu anda satışta değil.</p>
  }

  return (
    <ul className="divide-y rounded-lg border">
      {activeVariants.map((variant) => (
        <li key={variant.id} className="flex items-center justify-between gap-4 px-4 py-2.5 text-sm">
          <span>{variantLabel(variant)}</span>
          <span className="font-medium">{formatPrice(variant.price)}</span>
        </li>
      ))}
    </ul>
  )
}
