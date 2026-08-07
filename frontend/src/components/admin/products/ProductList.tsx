import Link from 'next/link'

import { Badge } from '@/components/ui/badge'
import type { Category, ProductSummary } from '@/features/catalog/types'
import { formatPrice } from '@/lib/format'

export function ProductList({ products, categories }: { products: ProductSummary[]; categories: Category[] }) {
  function categoryName(categoryId: string): string {
    return categories.find((c) => c.id === categoryId)?.name ?? '(bilinmiyor)'
  }

  return (
    <div className="space-y-2">
      {products.map((product) => (
        <Link
          key={product.id}
          href={`/admin/products/${product.id}`}
          className="flex items-center justify-between gap-3 rounded-lg border p-3 transition-colors hover:bg-muted"
        >
          <div className="flex items-center gap-3">
            {product.primaryImageUrl ? (
              // eslint-disable-next-line @next/next/no-img-element -- admin-entered arbitrary URLs, next/image's remotePatterns allowlist doesn't fit
              <img src={product.primaryImageUrl} alt="" className="size-10 rounded-md object-cover" />
            ) : (
              <div className="size-10 rounded-md bg-muted" />
            )}
            <div>
              <p className="font-medium">{product.name}</p>
              <p className="text-sm text-muted-foreground">{categoryName(product.categoryId)}</p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            {product.minPrice != null && <span className="text-sm">{formatPrice(product.minPrice)}</span>}
            {!product.isActive && <Badge variant="destructive">Pasif</Badge>}
          </div>
        </Link>
      ))}
    </div>
  )
}
