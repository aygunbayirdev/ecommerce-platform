import type { ProductSummary } from '@/features/catalog/types'

import { ProductCard } from './ProductCard'

export function ProductGrid({ products }: { products: ProductSummary[] }) {
  if (products.length === 0) {
    return (
      <p className="py-16 text-center text-muted-foreground">Bu kategoride henüz ürün yok.</p>
    )
  }

  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  )
}
