import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import type { StockItemWithProduct } from '@/features/inventory/types'

import { IncreaseStockDialog } from './IncreaseStockDialog'

export function StockTable({ items }: { items: StockItemWithProduct[] }) {
  if (items.length === 0) {
    return <p className="py-16 text-center text-muted-foreground">Henüz stok kaydı yok.</p>
  }

  return (
    <div className="space-y-2">
      {items.map((item) => (
        <div key={item.id} className="flex flex-wrap items-center justify-between gap-y-2 gap-x-3 rounded-lg border p-3">
          <div>
            <p className="font-medium">{item.productName}</p>
            <p className="text-sm text-muted-foreground">
              SKU: {item.sku}
              {!item.isVariantActive && (
                <>
                  {' '}
                  <Badge variant="destructive">Pasif Varyant</Badge>
                </>
              )}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <div className="text-right text-sm">
              <p>Müsait: {item.availableQuantity}</p>
              <p className="text-muted-foreground">Rezerve: {item.reservedQuantity}</p>
            </div>
            <IncreaseStockDialog
              productVariantId={item.productVariantId}
              productName={item.productName}
              trigger={
                <Button type="button" variant="outline" size="sm">
                  Stok Artır
                </Button>
              }
            />
          </div>
        </div>
      ))}
    </div>
  )
}
