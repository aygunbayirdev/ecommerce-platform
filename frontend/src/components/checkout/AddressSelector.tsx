import type { Address } from '@/features/addresses/types'
import { cn } from '@/lib/utils'

export function AddressSelector({
  addresses,
  selectedId,
  onSelect,
}: {
  addresses: Address[]
  selectedId: string | null
  onSelect: (addressId: string) => void
}) {
  return (
    <div className="space-y-2">
      {addresses.map((address) => (
        <label
          key={address.id}
          className={cn(
            'flex cursor-pointer items-start gap-3 rounded-lg border p-3',
            address.id === selectedId && 'border-primary bg-primary/5',
          )}
        >
          <input
            type="radio"
            name="checkout-address"
            className="mt-1"
            checked={address.id === selectedId}
            onChange={() => onSelect(address.id)}
          />
          <div className="space-y-0.5 text-sm">
            <p className="font-medium">{address.title}</p>
            <p className="text-muted-foreground">
              {address.recipientName} · {address.phoneNumber}
            </p>
            <p className="text-muted-foreground">
              {address.district}, {address.city}
            </p>
            <p className="text-muted-foreground">
              {address.fullAddressLine} — {address.postalCode}
            </p>
          </div>
        </label>
      ))}
    </div>
  )
}
