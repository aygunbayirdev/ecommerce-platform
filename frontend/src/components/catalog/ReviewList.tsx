import type { Review } from '@/features/reviews/types'

import { StarRating } from './StarRating'

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' })
}

export function ReviewList({ reviews }: { reviews: Review[] }) {
  if (reviews.length === 0) {
    return <p className="text-sm text-muted-foreground">Bu ürün için henüz yorum yapılmamış.</p>
  }

  const averageRating = reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <StarRating rating={Math.round(averageRating)} />
        <span className="text-sm text-muted-foreground">
          {averageRating.toFixed(1)} / 5 ({reviews.length} değerlendirme)
        </span>
      </div>

      <ul className="space-y-4">
        {reviews.map((review) => (
          <li key={review.id} className="space-y-1 rounded-lg border p-4">
            <div className="flex items-center justify-between gap-2">
              <StarRating rating={review.rating} />
              <span className="text-xs text-muted-foreground">{formatDate(review.createdAtUtc)}</span>
            </div>
            <p className="text-xs font-medium text-muted-foreground">Doğrulanmış Alıcı</p>
            <p className="text-sm">{review.comment}</p>
          </li>
        ))}
      </ul>
    </div>
  )
}
