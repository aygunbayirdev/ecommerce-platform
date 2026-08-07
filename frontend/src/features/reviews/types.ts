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

export type CreateReviewRequest = {
  productId: string
  orderId: string
  rating: number
  comment: string
}

// Admin moderation queue shape — enriched at the backend's Api layer (ReviewsController.GetPending)
// with product name / reviewer name, since the raw Review aggregate only stores ids.
export type ReviewAdmin = {
  id: string
  productId: string
  productName: string
  userId: string
  reviewerName: string
  orderId: string
  rating: number
  comment: string
  createdAtUtc: string
}
