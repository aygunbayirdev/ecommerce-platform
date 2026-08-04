import { AxiosError } from 'axios'
import { describe, expect, it } from 'vitest'

import { getApiErrorMessage } from './errors'

describe('getApiErrorMessage', () => {
  it('returns the ProblemDetails.Detail message from an Axios error response', () => {
    const error = new AxiosError('Request failed')
    error.response = {
      data: { detail: 'E-posta veya şifre hatalı.' },
      status: 401,
      statusText: 'Unauthorized',
      headers: {},
      config: {} as never,
    }

    expect(getApiErrorMessage(error)).toBe('E-posta veya şifre hatalı.')
  })

  it('falls back to the provided message when the Axios error has no Detail', () => {
    const error = new AxiosError('Request failed')
    error.response = {
      data: {},
      status: 500,
      statusText: 'Internal Server Error',
      headers: {},
      config: {} as never,
    }

    expect(getApiErrorMessage(error, 'Kayıt oluşturulamadı.')).toBe('Kayıt oluşturulamadı.')
  })

  it('falls back to the default message for non-Axios errors', () => {
    expect(getApiErrorMessage(new Error('boom'))).toBe('Bir hata oluştu. Lütfen tekrar deneyin.')
  })
})
