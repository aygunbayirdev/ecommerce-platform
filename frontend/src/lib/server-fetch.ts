// For Server Components hitting the monolith's anonymous GET endpoints directly (see
// CLAUDE.md madde 11 — public catalog pages are SSR for SEO, so this runs on the server,
// unrelated to lib/axios.ts's client-side, auth-aware instance).
const monolithBaseUrl = process.env.NEXT_PUBLIC_API_URL

export async function serverGet<T>(path: string): Promise<T | null> {
  const response = await fetch(`${monolithBaseUrl}${path}`)

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`GET ${path} failed with ${response.status}`)
  }

  return (await response.json()) as T
}
