'use client'

import { Trash2 } from 'lucide-react'
import { toast } from 'sonner'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useRemoveProductImage } from '@/features/catalog/api/useRemoveProductImage'
import type { ProductImage } from '@/features/catalog/types'

export function ImageGrid({ productId, images }: { productId: string; images: ProductImage[] }) {
  const removeImage = useRemoveProductImage()

  function handleRemove(imageId: string) {
    if (!window.confirm('Bu görseli kaldırmak istediğinize emin misiniz?')) {
      return
    }
    removeImage.mutate(
      { productId, imageId },
      { onError: () => toast.error('Görsel kaldırılamadı. Lütfen tekrar deneyin.') },
    )
  }

  if (images.length === 0) {
    return <p className="text-sm text-muted-foreground">Henüz görsel eklenmedi.</p>
  }

  return (
    <div className="grid grid-cols-3 gap-3 sm:grid-cols-4">
      {images.map((image) => (
        <div key={image.id} className="group relative overflow-hidden rounded-lg border">
          {/* eslint-disable-next-line @next/next/no-img-element -- admin-entered arbitrary URLs, next/image's remotePatterns allowlist doesn't fit */}
          <img src={image.url} alt="" className="aspect-square w-full object-cover" />
          {image.isPrimary && <Badge className="absolute top-1 left-1">Birincil</Badge>}
          <Button
            type="button"
            variant="destructive"
            size="icon-sm"
            aria-label="Görseli kaldır"
            className="absolute top-1 right-1"
            disabled={removeImage.isPending}
            onClick={() => handleRemove(image.id)}
          >
            <Trash2 className="size-3.5" />
          </Button>
        </div>
      ))}
    </div>
  )
}
