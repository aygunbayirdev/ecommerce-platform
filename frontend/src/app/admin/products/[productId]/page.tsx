'use client'

import { useParams } from 'next/navigation'
import { toast } from 'sonner'

import { AddImageDialog } from '@/components/admin/products/AddImageDialog'
import { AddVariantDialog } from '@/components/admin/products/AddVariantDialog'
import { ImageGrid } from '@/components/admin/products/ImageGrid'
import { VariantTable } from '@/components/admin/products/VariantTable'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useDeactivateProduct } from '@/features/catalog/api/useDeactivateProduct'
import { useProduct } from '@/features/catalog/api/useProduct'
import { useReactivateProduct } from '@/features/catalog/api/useReactivateProduct'

export default function AdminProductDetailPage() {
  const { productId } = useParams<{ productId: string }>()
  const { data: product, isPending, isError } = useProduct(productId)

  const deactivateProduct = useDeactivateProduct()
  const reactivateProduct = useReactivateProduct()

  function handleToggleActive() {
    if (!product) return
    const mutation = product.isActive ? deactivateProduct : reactivateProduct
    mutation.mutate(productId, {
      onError: () => toast.error('İşlem gerçekleştirilemedi. Lütfen tekrar deneyin.'),
    })
  }

  if (isPending) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (isError || !product) {
    return <p className="text-sm text-destructive">Ürün yüklenemedi. Lütfen sayfayı yenileyin.</p>
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <h1 className="text-2xl font-bold tracking-tight">{product.name}</h1>
          {!product.isActive && <Badge variant="destructive">Pasif</Badge>}
        </div>
        <Button
          type="button"
          variant="outline"
          disabled={deactivateProduct.isPending || reactivateProduct.isPending}
          onClick={handleToggleActive}
        >
          {product.isActive ? 'Ürünü Pasife Al' : 'Ürünü Aktifleştir'}
        </Button>
      </div>

      <p className="text-sm text-muted-foreground">{product.description}</p>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Varyantlar</h2>
          <AddVariantDialog
            productId={product.id}
            categoryId={product.categoryId}
            trigger={
              <Button type="button" variant="outline" size="sm">
                Varyant Ekle
              </Button>
            }
          />
        </div>
        <VariantTable productId={product.id} variants={product.variants} />
      </section>

      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold">Görseller</h2>
          <AddImageDialog
            productId={product.id}
            trigger={
              <Button type="button" variant="outline" size="sm">
                Görsel Ekle
              </Button>
            }
          />
        </div>
        <ImageGrid productId={product.id} images={product.images} />
      </section>
    </div>
  )
}
