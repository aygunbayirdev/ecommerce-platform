export type StockItemWithProduct = {
  id: string
  productVariantId: string
  productId: string
  productName: string
  sku: string
  availableQuantity: number
  reservedQuantity: number
  isVariantActive: boolean
  createdAtUtc: string
}
