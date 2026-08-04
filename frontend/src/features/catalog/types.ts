export type Category = {
  id: string
  name: string
  parentCategoryId: string | null
  displayOrder: number
  isActive: boolean
  createdAtUtc: string
}

export type ProductSummary = {
  id: string
  name: string
  categoryId: string
  brandId: string | null
  minPrice: number | null
  primaryImageUrl: string | null
  isActive: boolean
}

export type ProductVariantAttributeValue = {
  productAttributeId: string
  productAttributeName: string
  value: string
}

export type ProductVariant = {
  id: string
  sku: string
  price: number
  isActive: boolean
  attributeValues: ProductVariantAttributeValue[]
}

export type ProductImage = {
  id: string
  url: string
  isPrimary: boolean
  displayOrder: number
}

export type ProductDetail = {
  id: string
  categoryId: string
  brandId: string | null
  name: string
  description: string
  isActive: boolean
  createdAtUtc: string
  variants: ProductVariant[]
  images: ProductImage[]
}

export type PagedResult<T> = {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}
