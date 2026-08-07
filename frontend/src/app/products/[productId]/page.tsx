import { notFound } from 'next/navigation'

import { AddToCartForm } from '@/components/cart/AddToCartForm'
import { ProductGallery } from '@/components/catalog/ProductGallery'
import { ReviewList } from '@/components/catalog/ReviewList'
import { Separator } from '@/components/ui/separator'
import { getProductById } from '@/features/catalog/api/getProductById'
import { getApprovedReviews } from '@/features/reviews/api/getApprovedReviews'

// See app/page.tsx — same reasoning (always-fresh SSR, no build-time prerender attempt).
export const dynamic = 'force-dynamic'

export default async function ProductDetailPage({
  params,
}: {
  params: Promise<{ productId: string }>
}) {
  const { productId } = await params

  const [product, reviews] = await Promise.all([
    getProductById(productId),
    getApprovedReviews(productId),
  ])

  if (!product) {
    notFound()
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <div className="grid gap-8 md:grid-cols-2">
        <ProductGallery images={product.images} productName={product.name} />

        <div className="space-y-6">
          <div className="space-y-2">
            <h1 className="text-2xl font-bold tracking-tight">{product.name}</h1>
            <p className="whitespace-pre-line text-muted-foreground">{product.description}</p>
          </div>

          <AddToCartForm variants={product.variants} />
        </div>
      </div>

      <Separator className="my-8" />

      <div className="max-w-2xl space-y-4">
        <h2 className="text-lg font-semibold">Değerlendirmeler</h2>
        <ReviewList reviews={reviews.items} />
      </div>
    </div>
  )
}
