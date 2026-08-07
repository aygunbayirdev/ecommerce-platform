'use client'

import { Star } from 'lucide-react'

import { cn } from '@/lib/utils'

export function RatingInput({ value, onChange }: { value: number; onChange: (rating: number) => void }) {
  return (
    <div className="flex items-center gap-1" role="radiogroup" aria-label="Puan">
      {Array.from({ length: 5 }, (_, index) => {
        const rating = index + 1
        return (
          <button
            key={rating}
            type="button"
            role="radio"
            aria-checked={rating === value}
            aria-label={`${rating} yıldız`}
            onClick={() => onChange(rating)}
            className="p-0.5"
          >
            <Star
              className={cn(
                'size-6',
                rating <= value ? 'fill-primary text-primary' : 'text-muted-foreground',
              )}
            />
          </button>
        )
      })}
    </div>
  )
}
