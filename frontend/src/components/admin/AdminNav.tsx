'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'

import { cn } from '@/lib/utils'

const NAV_SECTIONS = [
  {
    title: 'Katalog',
    links: [
      { href: '/admin/categories', label: 'Kategoriler' },
      { href: '/admin/brands', label: 'Markalar' },
      { href: '/admin/attributes', label: 'Ürün Özellikleri' },
      { href: '/admin/products', label: 'Ürünler' },
      { href: '/admin/stock', label: 'Stok' },
    ],
  },
  {
    title: 'Operasyon',
    links: [
      { href: '/admin/orders', label: 'Siparişler' },
      { href: '/admin/coupons', label: 'Kuponlar' },
      { href: '/admin/reviews', label: 'Yorumlar' },
    ],
  },
]

export function AdminNav() {
  const pathname = usePathname()

  return (
    <nav className="flex flex-col gap-4 md:w-48 md:shrink-0 md:gap-6">
      {NAV_SECTIONS.map((section) => (
        <div key={section.title} className="space-y-1">
          <p className="px-2 text-xs font-medium tracking-wide text-muted-foreground uppercase">
            {section.title}
          </p>
          <div className="flex flex-wrap gap-1 md:flex-col">
            {section.links.map((link) => {
              const isActive = pathname === link.href || pathname.startsWith(`${link.href}/`)
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className={cn(
                    'rounded-md px-2 py-1.5 text-sm transition-colors md:block',
                    isActive ? 'bg-muted font-medium text-foreground' : 'text-muted-foreground hover:bg-muted',
                  )}
                >
                  {link.label}
                </Link>
              )
            })}
          </div>
        </div>
      ))}
    </nav>
  )
}
