import { createApiAuthService, createMockAuthService, type AuthService } from '~/services/authService'
import { createApiNotificationService, createMockNotificationService, type NotificationService } from '~/services/notificationService'
import { createApiOrnamentService, createMockOrnamentService, type OrnamentService } from '~/services/ornamentService'

export interface Services {
  auth: AuthService
  ornaments: OrnamentService
  notifications: NotificationService
}

let mockServices: Services | null = null

/**
 * Single switch between dummy data and the real API.
 * Set NUXT_PUBLIC_USE_MOCK_API=false to talk to the ASP.NET Core backend.
 */
export function useServices(): Services {
  const { useMockApi } = useRuntimeConfig().public

  if (useMockApi) {
    mockServices ??= {
      auth: createMockAuthService(),
      ornaments: createMockOrnamentService(),
      notifications: createMockNotificationService(),
    }
    return mockServices
  }

  const api = useApi()
  return {
    auth: createApiAuthService(api),
    ornaments: createApiOrnamentService(api),
    notifications: createApiNotificationService(api),
  }
}
