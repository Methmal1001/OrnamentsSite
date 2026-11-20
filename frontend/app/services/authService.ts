import type {
  AuthSession, ChangePasswordRequest, LoginRequest, NotificationPreferences, RegisterRequest, User, UserProfile,
} from '~/types'
import type { ApiClient } from '~/composables/useApi'
import { ApiError } from '~/composables/useApi'
import { seedUsers, type DemoUserRecord } from '~/data/users'
import { clone, createMockCollection, generateId, simulateLatency } from './mockDb'
import { readStorage, removeStorage, writeStorage } from '~/utils/storage'

export interface AuthService {
  login(request: LoginRequest): Promise<AuthSession>
  register(request: RegisterRequest): Promise<AuthSession>
  /** The current session, or null when signed out / expired. */
  getSession(): Promise<AuthSession | null>
  logout(): Promise<void>
  getProfile(userId: string): Promise<User>
  updateProfile(userId: string, profile: UserProfile): Promise<User>
  changePassword(userId: string, request: ChangePasswordRequest): Promise<void>
  getPreferences(userId: string): Promise<NotificationPreferences>
  updatePreferences(userId: string, preferences: NotificationPreferences): Promise<NotificationPreferences>
  listCustomers(): Promise<User[]>
}

// ---------------------------------------------------------------------------
// API implementation (ASP.NET Core). The JWT is an HttpOnly cookie set by the API.
// ---------------------------------------------------------------------------

export function createApiAuthService(api: ApiClient): AuthService {
  return {
    login: request => api.post<AuthSession>('/auth/login', request, { query: { useCookies: true }, skipAuthHandling: true }),
    register: request => api.post<AuthSession>('/auth/register', request, { query: { useCookies: true }, skipAuthHandling: true }),
    async getSession() {
      // 200 with the session, or 204 (empty body) when signed out.
      const session = await api.get<AuthSession | null | ''>('/auth/session', { skipAuthHandling: true })
      return session || null
    },
    logout: async () => { await api.post<void>('/auth/logout', undefined, { skipAuthHandling: true }) },
    getProfile: () => api.get<User>('/profile/me'),
    updateProfile: (_userId, profile) => api.put<User>('/profile/me', profile),
    changePassword: async (_userId, request) => { await api.post<void>('/profile/me/change-password', request) },
    getPreferences: () => api.get<NotificationPreferences>('/profile/me/notification-preferences'),
    updatePreferences: (_userId, preferences) => api.put<NotificationPreferences>('/profile/me/notification-preferences', preferences),
    listCustomers: () => api.get<User[]>('/admin/customers'),
  }
}

// ---------------------------------------------------------------------------
// Mock implementation (offline demo, NUXT_PUBLIC_USE_MOCK_API=true).
// NOT secure: demo users and the demo session live in browser storage.
// ---------------------------------------------------------------------------

const MOCK_SESSION_KEY = 'mock-session'
const defaultPreferences = (): NotificationPreferences => ({
  emailNotificationsEnabled: true, inAppNotificationsEnabled: true, updatedAt: new Date().toISOString(),
})

function toUser({ password: _password, ...user }: DemoUserRecord): User {
  return user
}

function mockError(message: string, status = 400, field?: string): ApiError {
  return new ApiError(message, status, field ? { [field]: message } : {})
}

export function createMockAuthService(): AuthService {
  const users = createMockCollection<DemoUserRecord>('users', seedUsers)
  const preferences = createMockCollection<{ userId: string } & NotificationPreferences>('preferences', [])

  const findIndexById = (all: DemoUserRecord[], id: string) => {
    const index = all.findIndex(u => u.id === id)
    if (index === -1) throw mockError('User not found', 404)
    return index
  }

  function startSession(user: User, remember: boolean): AuthSession {
    const session: AuthSession = {
      expiresAt: new Date(Date.now() + (remember ? 7 * 24 : 2) * 60 * 60 * 1000).toISOString(),
      user: clone(user),
    }
    removeStorage(MOCK_SESSION_KEY, 'local')
    removeStorage(MOCK_SESSION_KEY, 'session')
    writeStorage(MOCK_SESSION_KEY, session, remember ? 'local' : 'session')
    return session
  }

  function refreshStoredUser(user: User) {
    for (const kind of ['local', 'session'] as const) {
      const stored = readStorage<AuthSession | null>(MOCK_SESSION_KEY, null, kind)
      if (stored) writeStorage(MOCK_SESSION_KEY, { ...stored, user }, kind)
    }
  }

  return {
    async login({ email, password, rememberMe }) {
      const record = users.all().find(u => u.email.toLowerCase() === email.trim().toLowerCase())
      if (!record || record.password !== password) {
        await simulateLatency(null)
        throw mockError('Invalid email or password', 401)
      }
      return simulateLatency(startSession(toUser(record), rememberMe))
    },

    async register(request) {
      const all = users.all()
      if (all.some(u => u.email.toLowerCase() === request.email.trim().toLowerCase())) {
        await simulateLatency(null)
        throw mockError('An account with this email already exists', 409, 'email')
      }
      const record: DemoUserRecord = {
        id: generateId('usr'),
        fullName: request.fullName.trim(),
        email: request.email.trim(),
        phone: request.phone.trim(),
        address: request.address.trim(),
        password: request.password,
        role: 'customer',
        createdAt: new Date().toISOString(),
      }
      users.save([...all, record])
      return simulateLatency(startSession(toUser(record), false))
    },

    async getSession() {
      const session = readStorage<AuthSession | null>(MOCK_SESSION_KEY, null, 'local')
        ?? readStorage<AuthSession | null>(MOCK_SESSION_KEY, null, 'session')
      return session && new Date(session.expiresAt).getTime() > Date.now() ? session : null
    },

    async logout() {
      removeStorage(MOCK_SESSION_KEY, 'local')
      removeStorage(MOCK_SESSION_KEY, 'session')
    },

    async getProfile(userId) {
      const all = users.all()
      return simulateLatency(toUser(all[findIndexById(all, userId)]!))
    },

    async updateProfile(userId, profile) {
      const all = users.all()
      const index = findIndexById(all, userId)
      const emailTaken = all.some(u => u.id !== userId && u.email.toLowerCase() === profile.email.trim().toLowerCase())
      if (emailTaken) throw mockError('That email is already used by another account', 409, 'email')
      const updated = { ...all[index]!, ...profile }
      all[index] = updated
      users.save(all)
      refreshStoredUser(toUser(updated))
      return simulateLatency(toUser(updated))
    },

    async changePassword(userId, { currentPassword, newPassword }) {
      const all = users.all()
      const index = findIndexById(all, userId)
      if (all[index]!.password !== currentPassword) {
        await simulateLatency(null)
        throw mockError('Current password is incorrect', 400, 'currentPassword')
      }
      all[index] = { ...all[index]!, password: newPassword }
      users.save(all)
      await simulateLatency(null)
    },

    async getPreferences(userId) {
      const { userId: _id, ...prefs } = preferences.all().find(p => p.userId === userId) ?? { userId, ...defaultPreferences() }
      return simulateLatency(prefs, 150)
    },

    async updatePreferences(userId, prefs) {
      const saved = { userId, ...prefs, updatedAt: new Date().toISOString() }
      preferences.save([...preferences.all().filter(p => p.userId !== userId), saved])
      const { userId: _id, ...result } = saved
      return simulateLatency(result, 150)
    },

    async listCustomers() {
      return simulateLatency(users.all().filter(u => u.role === 'customer').map(toUser))
    },
  }
}
