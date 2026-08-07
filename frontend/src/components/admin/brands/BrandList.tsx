'use client'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import type { Brand } from '@/features/catalog/types'

import { BrandFormDialog } from './BrandFormDialog'

export function BrandList({ brands }: { brands: Brand[] }) {
  return (
    <div className="space-y-2">
      {brands.map((brand) => (
        <div key={brand.id} className="flex items-center justify-between gap-3 rounded-lg border p-3">
          <div className="flex items-center gap-2">
            <span className="font-medium">{brand.name}</span>
            {!brand.isActive && <Badge variant="destructive">Pasif</Badge>}
          </div>

          <BrandFormDialog
            brand={brand}
            trigger={
              <Button type="button" variant="outline" size="sm">
                Düzenle
              </Button>
            }
          />
        </div>
      ))}
    </div>
  )
}
