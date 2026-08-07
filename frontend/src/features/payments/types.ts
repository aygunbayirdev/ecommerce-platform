export type PaymentStatus = 'Pending' | 'Succeeded'

export type PaymentTransaction = {
  idempotencyKey: string
  isSuccessful: boolean
  failureReason: string | null
  occurredAtUtc: string
}

export type Payment = {
  id: string
  orderId: string
  userId: string
  amount: number
  status: PaymentStatus
  transactions: PaymentTransaction[]
}
