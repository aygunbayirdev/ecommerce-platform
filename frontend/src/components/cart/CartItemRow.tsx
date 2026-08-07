'use client'

import { Trash2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { useRemoveCartItem } from '@/features/cart/api/useRemoveCartItem'
import { useUpdateCartItemQuantity } from '@/features/cart/api/useUpdateCartItemQuantity'
import type { CartItem } from '@/features/cart/types'
import { formatPrice } from '@/lib/format'

export function CartItemRow({ item }: { item: CartItem }) {
  const updateQuantity = useUpdateCartItemQuantity()
  const removeItem = useRemoveCartItem()

  function handleQuantityChange(nextQuantity: number) {
    if (nextQuantity < 1) return
    updateQuantity.mutate({ productVariantId: item.productVariantId, quantity: nextQuantity })
  }

  return (
    <li className="flex flex-wrap items-center gap-3 py-4">
      <div className="flex min-w-0 flex-1 items-center gap-4">
        <div className="size-20 shrink-0 overflow-hidden rounded-lg bg-muted">
          {item.imageUrl && (
            // eslint-disable-next-line @next/next/no-img-element -- admin-entered arbitrary URLs, next/image's remotePatterns allowlist doesn't fit
            <img src={item.imageUrl} alt={item.productName} className="h-full w-full object-cover" />
          )}
        </div>

        <div className="min-w-0 space-y-0.5">
          <p className="truncate font-medium">{item.productName}</p>
          <p className="text-sm text-muted-foreground">{item.sku}</p>
          <p className="text-sm text-muted-foreground">{formatPrice(item.unitPrice)}</p>
        </div>
      </div>

      <div className="flex shrink-0 items-center gap-3">
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label="Adedi azalt"
            onClick={() => handleQuantityChange(item.quantity - 1)}
            disabled={updateQuantity.isPending || item.quantity <= 1}
          >
            −
          </Button>
          <span className="w-6 text-center text-sm">{item.quantity}</span>
          <Button
            type="button"
            variant="outline"
            size="icon"
            aria-label="Adedi artır"
            onClick={() => handleQuantityChange(item.quantity + 1)}
            disabled={updateQuantity.isPending}
          >
            +
          </Button>
        </div>

        <p className="w-24 text-right font-medium">{formatPrice(item.lineTotal)}</p>

        <Button
          type="button"
          variant="ghost"
          size="icon"
          aria-label="Sepetten kaldır"
          onClick={() => removeItem.mutate(item.productVariantId)}
          disabled={removeItem.isPending}
        >
          <Trash2 className="size-4" />
        </Button>
      </div>
    </li>
  )
}
