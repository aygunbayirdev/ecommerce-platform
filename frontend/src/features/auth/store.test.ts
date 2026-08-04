import { beforeEach, describe, expect, it } from 'vitest'

import { getCurrentUser, useAuthStore } from './store'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

function fakeJwt(payload: Record<string, unknown>): string {
  const encode = (obj: object) => btoa(JSON.stringify(obj)).replace(/=+$/, '')
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(payload)}.signature`
}

describe('auth store', () => {
  beforeEach(() => {
    useAuthStore.getState().clear()
  })

  it('has no current user before login', () => {
    expect(getCurrentUser()).toBeNull()
  })

  it('decodes the current user from the access token after setTokens', () => {
    const token = fakeJwt({
      sub: 'user-1',
      email: 'test@example.com',
      [ROLE_CLAIM]: 'Customer',
    })

    useAuthStore.getState().setTokens({
      userId: 'user-1',
      accessToken: token,
      accessTokenExpiresAtUtc: new Date().toISOString(),
      refreshToken: 'refresh-token',
    })

    expect(getCurrentUser()).toEqual({
      userId: 'user-1',
      email: 'test@example.com',
      role: 'Customer',
    })
  })

  it('clears the current user on logout', () => {
    useAuthStore.getState().setTokens({
      userId: 'user-1',
      accessToken: fakeJwt({ sub: 'user-1', email: 'a@b.com', [ROLE_CLAIM]: 'Admin' }),
      accessTokenExpiresAtUtc: new Date().toISOString(),
      refreshToken: 'refresh-token',
    })

    useAuthStore.getState().clear()

    expect(getCurrentUser()).toBeNull()
  })
})
