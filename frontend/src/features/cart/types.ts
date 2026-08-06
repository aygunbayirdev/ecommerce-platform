export type CartItem = {
  productVariantId: string
  quantity: number
  productName: string
  sku: string
  unitPrice: number
  imageUrl: string | null
  lineTotal: number
}

export type Cart = {
  id: string
  userId: string | null
  items: CartItem[]
  total: number
}
