import type { ProductAttribute } from '@/features/catalog/types'

export function AttributeList({ attributes }: { attributes: ProductAttribute[] }) {
  return (
    <div className="space-y-2">
      {attributes.map((attribute) => (
        <div key={attribute.id} className="rounded-lg border p-3">
          <span className="font-medium">{attribute.name}</span>
        </div>
      ))}
    </div>
  )
}
