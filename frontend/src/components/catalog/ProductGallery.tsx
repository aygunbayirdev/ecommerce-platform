'use client'

import { useState } from 'react'

import type { ProductImage } from '@/features/catalog/types'
import { cn } from '@/lib/utils'

export function ProductGallery({ images, productName }: { images: ProductImage[]; productName: string }) {
  const sorted = [...images].sort((a, b) => a.displayOrder - b.displayOrder)
  const [selected, setSelected] = useState(sorted[0]?.id)
  const activeImage = sorted.find((image) => image.id === selected) ?? sorted[0]

  if (sorted.length === 0) {
    return (
      <div className="flex aspect-square w-full items-center justify-center rounded-xl bg-muted text-muted-foreground">
        Görsel yok
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {/* eslint-disable-next-line @next/next/no-img-element -- admin-entered arbitrary URLs, next/image's remotePatterns allowlist doesn't fit */}
      <img
        src={activeImage.url}
        alt={productName}
        className="aspect-square w-full rounded-xl object-cover"
      />

      {sorted.length > 1 && (
        <div className="flex gap-2">
          {sorted.map((image) => (
            <button
              key={image.id}
              type="button"
              onClick={() => setSelected(image.id)}
              className={cn(
                'size-16 shrink-0 overflow-hidden rounded-lg ring-1 ring-foreground/10',
                image.id === activeImage.id && 'ring-2 ring-primary',
              )}
            >
              {/* eslint-disable-next-line @next/next/no-img-element -- admin-entered arbitrary URLs */}
              <img src={image.url} alt="" className="h-full w-full object-cover" />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
