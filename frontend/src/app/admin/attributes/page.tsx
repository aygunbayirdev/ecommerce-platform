'use client'

import { AttributeFormDialog } from '@/components/admin/attributes/AttributeFormDialog'
import { AttributeList } from '@/components/admin/attributes/AttributeList'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useProductAttributes } from '@/features/catalog/api/useProductAttributes'

export default function AdminAttributesPage() {
  const { data: attributes, isPending, isError } = useProductAttributes()

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Ürün Özellikleri</h1>
        <AttributeFormDialog trigger={<Button type="button">Yeni Özellik Ekle</Button>} />
      </div>

      {isPending && (
        <div className="space-y-2">
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-14 w-full" />
        </div>
      )}

      {isError && <p className="text-sm text-destructive">Özellikler yüklenemedi. Lütfen sayfayı yenileyin.</p>}

      {attributes && attributes.length === 0 && (
        <p className="py-16 text-center text-muted-foreground">Henüz özellik yok.</p>
      )}

      {attributes && attributes.length > 0 && <AttributeList attributes={attributes} />}
    </div>
  )
}
