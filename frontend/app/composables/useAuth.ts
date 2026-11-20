import type { AppNotification, AuthSession, ChangePasswordRequest, LoginRequest, RegisterRequest, UserProfile } from '~/types'

/**
 * Session state, held in memory only. With the real API the credential is an HttpOnly
 * cookie managed by the browser; the frontend only knows who is signed in and until when.
 * It is restored from GET /api/auth/session when the app starts (plugins/auth.ts).
 */
export const useSession = () => useState<AuthSession | null>('auth-session', () => null)

/**
 * Authentication for the UI. Route guards and role checks here are conveniences for
 * navigation only. Every permission is enforced again by the API.
 */
export function useAuth() {
  const session = useSession()
  const { auth } = useServices()

  const user = computed(() => session.value?.user ?? null)
  const isAuthenticated = computed(() => session.value !== null && !isExpired(session.value))
  const isAdmin = computed(() => user.value?.role === 'admin')
  const homeRoute = computed(() => (isAdmin.value ? '/admin' : '/dashboard'))

  function isExpired(value: AuthSession) {
    return new Date(value.expiresAt).getTime() <= Date.now()
  }

  function clearUserState() {
    session.value = null
    useState<AppNotification[]>('notifications').value = []
    useState<string | null>('notifications-owner').value = null
    useState('notification-preferences').value = null
  }

  /** Restores the session from the server (cookie) or the mock store. Never throws. */
  async function restoreSession() {
    try {
      session.value = await auth.getSession()
    }
    catch (error) {
      session.value = null
      useToast().error(errorMessage(error, 'Could not check your sign-in status.'))
    }
  }

  async function login(request: LoginRequest) {
    session.value = await auth.login(request)
    return session.value.user
  }

  async function register(request: RegisterRequest) {
    session.value = await auth.register(request)
    return session.value.user
  }

  async function logout() {
    try {
      await auth.logout()
    }
    catch {
      // Even if the server is unreachable, forget the session locally.
    }
    clearUserState()
    await navigateTo('/login')
  }

  /**
   * Called when the API returns 401 for an authenticated request, or when the session's
   * expiry time passes. Signs out locally and sends the user to the login page.
   */
  async function expireSession() {
    if (!session.value) return
    clearUserState()
    useToast().info(SESSION_EXPIRED_MESSAGE)
    // Only leave the page if it requires a signed-in user; public pages stay put.
    const route = useRoute()
    if (route.meta.middleware) {
      await navigateTo({ path: '/login', query: { redirect: route.fullPath } })
    }
  }

  /** Reloads the user from GET /api/profile/me so the profile page shows server data. */
  async function refreshProfile() {
    if (!session.value) throw new Error('You are not signed in')
    const fresh = await auth.getProfile(session.value.user.id)
    if (session.value) session.value = { ...session.value, user: fresh }
    return fresh
  }

  async function updateProfile(profile: UserProfile) {
    if (!session.value) throw new Error('You are not signed in')
    const updated = await auth.updateProfile(session.value.user.id, profile)
    if (session.value) session.value = { ...session.value, user: updated }
    return updated
  }

  async function changePassword(request: ChangePasswordRequest) {
    if (!session.value) throw new Error('You are not signed in')
    await auth.changePassword(session.value.user.id, request)
  }

  return {
    session, user, isAuthenticated, isAdmin, homeRoute,
    restoreSession, login, register, logout, expireSession, refreshProfile, updateProfile, changePassword,
  }
}
