import Link from 'next/link'

import { Button } from '@/components/ui/button'

export default function NotFound() {
  return (
    <div className="mx-auto max-w-3xl space-y-4 px-4 py-16 text-center">
      <h1 className="text-2xl font-bold tracking-tight">Sayfa bulunamadı</h1>
      <p className="text-muted-foreground">Aradığınız sayfa mevcut değil ya da kaldırılmış olabilir.</p>
      <Button render={<Link href="/" />} nativeButton={false}>
        Ana Sayfaya Dön
      </Button>
    </div>
  )
}
