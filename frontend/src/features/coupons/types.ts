// Backend has no [JsonStringEnumConverter] registered, so CouponDiscountType serializes as a
// plain number: 0 = Percentage, 1 = FixedAmount (see backend Coupon.Domain/CouponDiscountType.cs).
export type CouponDiscountType = 0 | 1

export type Coupon = {
  id: string
  code: string
  discountType: CouponDiscountType
  discountValue: number
  validFrom: string
  validTo: string
  usageLimit: number | null
  usedCount: number
  isActive: boolean
  createdAtUtc: string
}

export type CreateCouponPayload = {
  code: string
  discountType: CouponDiscountType
  discountValue: number
  validFrom: string
  validTo: string
  usageLimit: number | null
}
