export type Review = {
  id: string
  productId: string
  userId: string
  orderId: string
  rating: number
  comment: string
  isApproved: boolean
  createdAtUtc: string
}
