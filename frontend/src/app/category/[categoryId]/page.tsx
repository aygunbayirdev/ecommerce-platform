import { notFound } from 'next/navigation'

import { CategoryNav } from '@/components/catalog/CategoryNav'
import { ProductGrid } from '@/components/catalog/ProductGrid'
import { getCategories } from '@/features/catalog/api/getCategories'
import { getProducts } from '@/features/catalog/api/getProducts'

export default async function CategoryPage({
  params,
}: {
  params: Promise<{ categoryId: string }>
}) {
  const { categoryId } = await params

  const [categories, products] = await Promise.all([
    getCategories(),
    getProducts({ categoryId }),
  ])

  const category = categories.find((c) => c.id === categoryId)

  if (!category) {
    notFound()
  }

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-8">
      <div className="space-y-2">
        <h1 className="text-2xl font-bold tracking-tight">{category.name}</h1>
        <CategoryNav categories={categories} activeCategoryId={category.id} />
      </div>

      <ProductGrid products={products.items} />
    </div>
  )
}
