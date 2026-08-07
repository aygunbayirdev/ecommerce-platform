'use client'

import { Button } from '@/components/ui/button'

export default function Error({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <div className="mx-auto max-w-3xl space-y-4 px-4 py-16 text-center">
      <h1 className="text-2xl font-bold tracking-tight">Bir şeyler ters gitti</h1>
      <p className="text-muted-foreground">Sayfa yüklenirken bir hata oluştu. Lütfen tekrar deneyin.</p>
      <Button type="button" onClick={() => reset()}>
        Tekrar Dene
      </Button>
    </div>
  )
}
