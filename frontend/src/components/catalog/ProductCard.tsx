import Link from 'next/link'

import { Card, CardContent, CardTitle } from '@/components/ui/card'
import type { ProductSummary } from '@/features/catalog/types'
import { formatPrice } from '@/lib/format'

export function ProductCard({ product }: { product: ProductSummary }) {
  return (
    <Link href={`/products/${product.id}`}>
      <Card className="h-full transition-shadow hover:shadow-md">
        <div className="aspect-square w-full overflow-hidden bg-muted">
          {product.primaryImageUrl ? (
            // eslint-disable-next-line @next/next/no-img-element -- admin-entered arbitrary URLs, next/image's remotePatterns allowlist doesn't fit
            <img
              src={product.primaryImageUrl}
              alt={product.name}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-sm text-muted-foreground">
              Görsel yok
            </div>
          )}
        </div>
        <CardContent className="space-y-1">
          <CardTitle className="line-clamp-2">{product.name}</CardTitle>
          <p className="text-sm font-medium text-foreground">
            {product.minPrice !== null ? formatPrice(product.minPrice) : 'Fiyat bilgisi yok'}
          </p>
        </CardContent>
      </Card>
    </Link>
  )
}
