import type { OrderStatusHistoryEntry } from '@/features/orders/types'
import { orderStatusLabel } from '@/lib/orderStatus'

function formatDateTime(isoDate: string): string {
  return new Date(isoDate).toLocaleString('tr-TR', { dateStyle: 'medium', timeStyle: 'short' })
}

export function OrderStatusHistoryTimeline({ entries }: { entries: OrderStatusHistoryEntry[] }) {
  return (
    <ol className="space-y-3">
      {entries.map((entry, index) => (
        <li key={`${entry.status}-${entry.changedAtUtc}-${index}`} className="flex items-start gap-3">
          <div className="mt-1.5 size-2 shrink-0 rounded-full bg-primary" />
          <div>
            <p className="text-sm font-medium">{orderStatusLabel(entry.status)}</p>
            <p className="text-xs text-muted-foreground">{formatDateTime(entry.changedAtUtc)}</p>
            {entry.note && <p className="text-sm text-muted-foreground">{entry.note}</p>}
          </div>
        </li>
      ))}
    </ol>
  )
}
