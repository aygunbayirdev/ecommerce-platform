import Link from 'next/link'

import { Card } from '@/components/ui/card'

const SHORTCUTS = [
  { href: '/admin/categories', label: 'Kategoriler', description: 'Kategori oluştur, yeniden adlandır, özellik ata.' },
  { href: '/admin/brands', label: 'Markalar', description: 'Marka oluştur, yeniden adlandır.' },
  { href: '/admin/attributes', label: 'Ürün Özellikleri', description: 'Varyantlarda kullanılacak özellikleri tanımla.' },
  { href: '/admin/products', label: 'Ürünler', description: 'Ürün oluştur, varyant/görsel ekle, aktif/pasif yap.' },
  { href: '/admin/stock', label: 'Stok', description: 'Varyant bazlı stok seviyelerini görüntüle, artır.' },
]

export default function AdminHomePage() {
  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold tracking-tight">Admin Panel</h1>

      <div className="grid gap-4 sm:grid-cols-2">
        {SHORTCUTS.map((shortcut) => (
          <Link key={shortcut.href} href={shortcut.href}>
            <Card className="p-4 transition-colors hover:bg-muted">
              <p className="font-medium">{shortcut.label}</p>
              <p className="text-sm text-muted-foreground">{shortcut.description}</p>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  )
}
