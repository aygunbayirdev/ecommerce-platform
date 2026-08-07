import { formatPrice } from '@/lib/format'

export type NormalizedOrderLine = {
  key: string
  name: string
  sku: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export function OrderItemsList({ items, total }: { items: NormalizedOrderLine[]; total: number }) {
  return (
    <div className="space-y-3">
      <ul className="divide-y rounded-lg border">
        {items.map((item) => (
          <li key={item.key} className="flex items-center justify-between gap-4 px-4 py-2.5 text-sm">
            <div>
              <p className="font-medium">{item.name}</p>
              <p className="text-muted-foreground">
                {item.sku} × {item.quantity}
              </p>
            </div>
            <span className="font-medium">{formatPrice(item.lineTotal)}</span>
          </li>
        ))}
      </ul>

      <div className="flex items-center justify-between border-t pt-3">
        <span className="text-lg font-semibold">Toplam</span>
        <span className="text-lg font-semibold">{formatPrice(total)}</span>
      </div>
    </div>
  )
}
