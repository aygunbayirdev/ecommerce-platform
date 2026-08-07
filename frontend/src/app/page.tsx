import { CategoryNav } from '@/components/catalog/CategoryNav'
import { ProductGrid } from '@/components/catalog/ProductGrid'
import { getCategories } from '@/features/catalog/api/getCategories'
import { getProducts } from '@/features/catalog/api/getProducts'

// Always render per-request — otherwise `next build` tries to prerender this at build time,
// which fails outright in Docker (the backend container isn't reachable during image build).
export const dynamic = 'force-dynamic'

export default async function HomePage() {
  const [categories, products] = await Promise.all([getCategories(), getProducts({})])

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-8">
      <div className="space-y-2">
        <h1 className="text-2xl font-bold tracking-tight">Ürünler</h1>
        <CategoryNav categories={categories} />
      </div>

      <ProductGrid products={products.items} />
    </div>
  )
}
