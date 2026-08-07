'use client'

import { type FormEvent, useState } from 'react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useProcessPayment } from '@/features/payments/api/useProcessPayment'
import { getApiErrorMessage } from '@/lib/errors'

export function PaymentForm({ orderId, onSuccess }: { orderId: string; onSuccess: () => void }) {
  const [cardNumber, setCardNumber] = useState('')
  const processPayment = useProcessPayment()

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    processPayment.mutate({ orderId, cardNumber }, { onSuccess })
  }

  return (
    <form onSubmit={handleSubmit} className="max-w-sm space-y-4 rounded-lg border p-4">
      <div className="space-y-1.5">
        <Label htmlFor="card-number">Kart Numarası</Label>
        <Input
          id="card-number"
          required
          inputMode="numeric"
          placeholder="4242 4242 4242 4242"
          value={cardNumber}
          onChange={(event) => setCardNumber(event.target.value)}
        />
        <p className="text-xs text-muted-foreground">
          Test modu: &quot;0000&quot; ile biten kart numaraları reddedilir, diğer tüm numaralar onaylanır.
        </p>
      </div>

      {processPayment.isError && (
        <p className="text-sm text-destructive">
          {getApiErrorMessage(processPayment.error, 'Ödeme alınamadı. Lütfen tekrar deneyin.')}
        </p>
      )}

      <Button type="submit" className="w-full" disabled={processPayment.isPending}>
        {processPayment.isPending ? 'Ödeme alınıyor...' : 'Öde'}
      </Button>
    </form>
  )
}
