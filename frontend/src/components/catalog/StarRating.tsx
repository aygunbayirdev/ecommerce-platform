import { Star } from 'lucide-react'

import { cn } from '@/lib/utils'

export function StarRating({ rating, className }: { rating: number; className?: string }) {
  return (
    <div className={cn('flex items-center gap-0.5', className)} aria-label={`${rating} / 5 yıldız`}>
      {Array.from({ length: 5 }, (_, index) => (
        <Star
          key={index}
          className={cn(
            'size-4',
            index < rating ? 'fill-primary text-primary' : 'text-muted-foreground',
          )}
        />
      ))}
    </div>
  )
}
