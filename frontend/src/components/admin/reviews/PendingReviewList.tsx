'use client'

import { toast } from 'sonner'

import { StarRating } from '@/components/catalog/StarRating'
import { Button } from '@/components/ui/button'
import { useApproveReview } from '@/features/reviews/api/useApproveReview'
import { useRejectReview } from '@/features/reviews/api/useRejectReview'
import type { ReviewAdmin } from '@/features/reviews/types'

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' })
}

export function PendingReviewList({ reviews }: { reviews: ReviewAdmin[] }) {
  const approveReview = useApproveReview()
  const rejectReview = useRejectReview()

  function handleApprove(reviewId: string) {
    approveReview.mutate(reviewId, {
      onError: () => toast.error('Yorum onaylanamadı. Lütfen tekrar deneyin.'),
    })
  }

  function handleReject(reviewId: string) {
    if (!window.confirm('Bu yorumu reddetmek (kalıcı olarak silmek) istediğinize emin misiniz?')) {
      return
    }
    rejectReview.mutate(reviewId, {
      onError: () => toast.error('Yorum reddedilemedi. Lütfen tekrar deneyin.'),
    })
  }

  return (
    <div className="space-y-3">
      {reviews.map((review) => (
        <div key={review.id} className="space-y-2 rounded-lg border p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">{review.productName}</p>
              <p className="text-sm text-muted-foreground">
                {review.reviewerName} · {formatDate(review.createdAtUtc)}
              </p>
            </div>
            <StarRating rating={review.rating} />
          </div>

          <p className="text-sm">{review.comment}</p>

          <div className="flex items-center gap-2 pt-1">
            <Button
              type="button"
              size="sm"
              disabled={approveReview.isPending}
              onClick={() => handleApprove(review.id)}
            >
              Onayla
            </Button>
            <Button
              type="button"
              variant="destructive"
              size="sm"
              disabled={rejectReview.isPending}
              onClick={() => handleReject(review.id)}
            >
              Reddet
            </Button>
          </div>
        </div>
      ))}
    </div>
  )
}
