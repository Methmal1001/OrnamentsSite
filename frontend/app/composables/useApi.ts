import type { ApiProblem } from '~/types'

/**
 * HTTP client for the ASP.NET Core API.
 *
 * - Base URL from runtime config (NUXT_PUBLIC_API_BASE_URL).
 * - `credentials: 'include'` sends the HttpOnly session cookie; no token is ever read or stored by JS.
 * - `X-Requested-With` satisfies the API's anti-CSRF check for state-changing requests.
 * - Every failure becomes an {@link ApiError} with a user-friendly message and per-field errors.
 * - A 401 on an authenticated call means the session expired or was revoked → sign out and redirect.
 */

export type ApiErrorKind = 'http' | 'network' | 'timeout'

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    /** Field errors keyed by camelCase property name, e.g. `{ email: 'Enter a valid email' }`. */
    readonly fieldErrors: Record<string, string> = {},
    readonly kind: ApiErrorKind = 'http',
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

export interface RequestOptions {
  query?: Record<string, string | number | boolean | undefined>
  /** Don't treat a 401 as an expired session (login, session probe). */
  skipAuthHandling?: boolean
}

export interface ApiClient {
  get<T>(url: string, options?: RequestOptions): Promise<T>
  post<T>(url: string, body?: object, options?: RequestOptions): Promise<T>
  put<T>(url: string, body?: object, options?: RequestOptions): Promise<T>
  delete<T>(url: string, options?: RequestOptions): Promise<T>
}

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE'

export const SESSION_EXPIRED_MESSAGE = 'Your session has expired. Please sign in again.'
const NETWORK_MESSAGE = 'Can’t reach the Lanka Ornaments server. Check your internet connection and try again.'

/** "FullName" / "$.price" / "Errors[0].Name" → "fullName" / "price" / "name". */
function toFieldName(key: string): string {
  const last = key.replace(/^\$\.?/, '').split('.').pop() ?? key
  const clean = last.replace(/\[\d+\]/g, '')
  return clean.charAt(0).toLowerCase() + clean.slice(1)
}

function friendlyStatusMessage(status: number): string {
  if (status === 401) return SESSION_EXPIRED_MESSAGE
  if (status === 403) return 'You don’t have permission to do that.'
  if (status === 404) return 'We couldn’t find what you were looking for.'
  if (status === 429) return 'Too many attempts. Please wait a minute and try again.'
  if (status >= 500) return 'Something went wrong on our side. Please try again shortly.'
  return 'The request could not be completed.'
}

function toApiError(error: unknown): ApiError {
  const err = error as { name?: string, statusCode?: number, response?: unknown, data?: ApiProblem } | null
  if (err?.name === 'AbortError' || err?.name === 'TimeoutError') {
    return new ApiError('The server took too long to respond. Please try again.', 0, {}, 'timeout')
  }
  if (!err?.response || !err.statusCode) {
    return new ApiError(NETWORK_MESSAGE, 0, {}, 'network')
  }

  const status = err.statusCode
  const problem = err.data ?? {}
  const fieldErrors: Record<string, string> = {}
  for (const [key, messages] of Object.entries(problem.errors ?? {})) {
    const first = messages?.[0]
    if (first && key) fieldErrors[toFieldName(key)] ??= first
  }

  const message = status === 401 && !problem.title?.startsWith('Invalid') && !problem.title?.startsWith('Too many')
    ? SESSION_EXPIRED_MESSAGE
    : Object.values(fieldErrors)[0] ?? problem.detail ?? problem.title ?? friendlyStatusMessage(status)

  return new ApiError(message, status, fieldErrors)
}

export function useApi(): ApiClient {
  const config = useRuntimeConfig()
  const nuxtApp = useNuxtApp()

  const client = $fetch.create({
    baseURL: config.public.apiBaseUrl,
    credentials: 'include',
    timeout: 15000,
    headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' },
  })

  async function request<T>(method: HttpMethod, url: string, body?: object, options: RequestOptions = {}): Promise<T> {
    try {
      return await (client(url, { method, body, query: options.query }) as Promise<T>)
    }
    catch (error) {
      const apiError = toApiError(error)
      if (apiError.status === 401 && !options.skipAuthHandling) {
        await nuxtApp.runWithContext(() => useAuth().expireSession())
      }
      throw apiError
    }
  }

  return {
    get: (url, options) => request('GET', url, undefined, options),
    post: (url, body, options) => request('POST', url, body, options),
    put: (url, body, options) => request('PUT', url, body, options),
    delete: (url, options) => request('DELETE', url, undefined, options),
  }
}
