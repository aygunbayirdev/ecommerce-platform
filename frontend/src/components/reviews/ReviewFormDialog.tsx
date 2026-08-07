'use client'

import { type FormEvent, useState } from 'react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { useCreateReview } from '@/features/reviews/api/useCreateReview'
import { getApiErrorMessage } from '@/lib/errors'

import { RatingInput } from './RatingInput'

export function ReviewFormDialog({
  productId,
  orderId,
  productName,
  trigger,
}: {
  productId: string
  orderId: string
  productName: string
  trigger: React.ReactElement
}) {
  const [open, setOpen] = useState(false)
  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')
  const createReview = useCreateReview()

  function handleOpenChange(nextOpen: boolean) {
    setOpen(nextOpen)
    if (nextOpen) {
      setRating(5)
      setComment('')
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    createReview.mutate(
      { productId, orderId, rating, comment },
      {
        onSuccess: () => {
          toast.success('Yorumunuz gönderildi, onaylandıktan sonra yayınlanacak.')
          setOpen(false)
        },
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogTrigger render={trigger} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{productName} — Yorum Yap</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div className="space-y-1.5">
            <Label>Puan</Label>
            <RatingInput value={rating} onChange={setRating} />
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="review-comment">Yorumunuz</Label>
            <Textarea
              id="review-comment"
              required
              maxLength={2000}
              value={comment}
              onChange={(event) => setComment(event.target.value)}
            />
          </div>

          {createReview.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(createReview.error, 'Yorum gönderilemedi. Lütfen tekrar deneyin.')}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={createReview.isPending}>
              {createReview.isPending ? 'Gönderiliyor...' : 'Yorumu Gönder'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
