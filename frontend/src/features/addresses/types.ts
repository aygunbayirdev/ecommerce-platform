export type Address = {
  id: string
  title: string
  recipientName: string
  phoneNumber: string
  city: string
  district: string
  fullAddressLine: string
  postalCode: string
  isDefault: boolean
  createdAtUtc: string
}

// Shared by add and update — the backend infers "isDefault" from context (the very first
// address a user adds becomes default automatically) rather than accepting it as a client choice
// here; switching the default afterward always goes through the separate "set default" endpoint.
export type AddressFormValues = {
  title: string
  recipientName: string
  phoneNumber: string
  city: string
  district: string
  fullAddressLine: string
  postalCode: string
}
