export type AuthTokens = {
  userId: string
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
}

export type RegisterRequest = {
  email: string
  password: string
  firstName: string
  lastName: string
  phoneNumber?: string
}

export type LoginRequest = {
  email: string
  password: string
}

// Decoded from the JWT "sub"/role claims after login — enough to gate the admin area
// and show the right header UI without a dedicated "current user" endpoint round-trip.
export type CurrentUser = {
  userId: string
  email: string
  role: 'Customer' | 'Admin'
}
