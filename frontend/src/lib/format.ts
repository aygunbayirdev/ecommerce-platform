const currencyFormatter = new Intl.NumberFormat('tr-TR', {
  style: 'currency',
  currency: 'TRY',
  minimumFractionDigits: 2,
})

export function formatPrice(amount: number): string {
  return currencyFormatter.format(amount)
}
