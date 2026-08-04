import { AxiosError } from 'axios'

const DEFAULT_FALLBACK = 'Bir hata oluştu. Lütfen tekrar deneyin.'

// The backend's ResultExtensions.ToProblemDetails() always puts a ready-to-show Turkish
// message in ProblemDetails.Detail (see e.g. LoginCommandHandler's Error.Unauthorized(...)
// calls) — no need to duplicate a code->message lookup table on the frontend.
export function getApiErrorMessage(error: unknown, fallback: string = DEFAULT_FALLBACK): string {
  if (error instanceof AxiosError) {
    const detail = (error.response?.data as { detail?: string } | undefined)?.detail
    if (detail) {
      return detail
    }
  }

  return fallback
}
