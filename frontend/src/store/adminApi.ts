import { apiUrl } from './api'

let antiforgeryToken = ''

export async function adminMutation<T>(path: string, method: string, body?: BodyInit, headers?: HeadersInit): Promise<T> {
  if (!antiforgeryToken) {
    const tokenResponse = await fetch(`${apiUrl}/admin/security/antiforgery`, { credentials: 'include' })
    if (!tokenResponse.ok) throw new Error('Your admin session has expired.')
    antiforgeryToken = ((await tokenResponse.json()) as { token: string }).token
  }
  const response = await fetch(`${apiUrl}${path}`, { method, body, credentials: 'include', headers: { 'X-XSRF-TOKEN': antiforgeryToken, ...headers } })
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { detail?: string; title?: string } | null
    throw new Error(problem?.detail ?? problem?.title ?? 'The operation could not be completed.')
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
