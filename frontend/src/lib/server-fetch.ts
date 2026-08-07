// For Server Components hitting the monolith's anonymous GET endpoints directly (see
// CLAUDE.md madde 11 — public catalog pages are SSR for SEO, so this runs on the server,
// unrelated to lib/axios.ts's client-side, auth-aware instance).
//
// INTERNAL_API_URL (no NEXT_PUBLIC_ prefix, so it's never inlined into the client bundle) is set
// only when running in Docker (docker-compose.yml), pointing at the backend container's service
// name (`http://backend:8080/api`) — this SSR fetch runs inside the frontend container's own
// Node process, where `localhost` would resolve to itself, not the backend container. Outside
// Docker (plain `npm run dev`), it's unset and this falls back to NEXT_PUBLIC_API_URL, unchanged.
const monolithBaseUrl = process.env.INTERNAL_API_URL ?? process.env.NEXT_PUBLIC_API_URL

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
