import { Badge } from '@/components/ui/badge'
import type { Shipment } from '@/features/shipments/types'
import { shipmentStatusLabel } from '@/lib/shipmentStatus'

function formatDateTime(isoDate: string): string {
  return new Date(isoDate).toLocaleString('tr-TR', { dateStyle: 'medium', timeStyle: 'short' })
}

export function ShipmentInfo({ shipment }: { shipment: Shipment | null | undefined }) {
  if (!shipment) {
    return <p className="text-sm text-muted-foreground">Henüz kargoya verilmedi.</p>
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <Badge variant={shipment.status === 'Failed' ? 'destructive' : 'secondary'}>
          {shipmentStatusLabel(shipment.status)}
        </Badge>
        <span className="text-sm text-muted-foreground">
          {shipment.carrier} · {shipment.trackingNumber}
        </span>
      </div>

      {shipment.failureReason && (
        <p className="text-sm text-destructive">{shipment.failureReason}</p>
      )}

      <ol className="space-y-3">
        {shipment.statusHistory.map((entry, index) => (
          <li key={`${entry.status}-${entry.changedAtUtc}-${index}`} className="flex items-start gap-3">
            <div className="mt-1.5 size-2 shrink-0 rounded-full bg-primary" />
            <div>
              <p className="text-sm font-medium">{shipmentStatusLabel(entry.status)}</p>
              <p className="text-xs text-muted-foreground">{formatDateTime(entry.changedAtUtc)}</p>
              {entry.note && <p className="text-sm text-muted-foreground">{entry.note}</p>}
            </div>
          </li>
        ))}
      </ol>
    </div>
  )
}
