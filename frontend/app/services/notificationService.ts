import type { AppNotification, CreateNotificationRequest } from '~/types'
import type { ApiClient } from '~/composables/useApi'
import { seedNotifications } from '~/data/notifications'
import { createMockCollection, generateId, simulateLatency } from './mockDb'

export interface NotificationService {
  /** Notifications for one customer, newest first. */
  listForUser(userId: string): Promise<AppNotification[]>
  /** Every notification (admin view), newest first. */
  listAll(): Promise<AppNotification[]>
  markAsRead(id: string): Promise<void>
  markAllAsRead(userId: string): Promise<void>
  remove(id: string): Promise<void>
  /** `recipientIds` is the resolved list of customers to notify. */
  create(request: CreateNotificationRequest, recipientIds: string[]): Promise<AppNotification[]>
}

const newestFirst = (a: AppNotification, b: AppNotification) => b.createdAt.localeCompare(a.createdAt)

export function createMockNotificationService(): NotificationService {
  const notifications = createMockCollection<AppNotification>('notifications', seedNotifications)

  const updateWhere = (predicate: (n: AppNotification) => boolean, patch: Partial<AppNotification>) => {
    notifications.save(notifications.all().map(n => (predicate(n) ? { ...n, ...patch } : n)))
  }

  return {
    listForUser: userId => simulateLatency(notifications.all().filter(n => n.userId === userId).sort(newestFirst)),

    listAll: () => simulateLatency(notifications.all().sort(newestFirst)),

    async markAsRead(id) {
      updateWhere(n => n.id === id, { isRead: true })
      await simulateLatency(null, 120)
    },

    async markAllAsRead(userId) {
      updateWhere(n => n.userId === userId, { isRead: true })
      await simulateLatency(null, 120)
    },

    async remove(id) {
      notifications.save(notifications.all().filter(n => n.id !== id))
      await simulateLatency(null, 120)
    },

    async create({ title, message, type }, recipientIds) {
      const createdAt = new Date().toISOString()
      const created = recipientIds.map<AppNotification>(userId => ({
        id: generateId('ntf'), userId, title: title.trim(), message: message.trim(), type, isRead: false, createdAt,
      }))
      notifications.save([...created, ...notifications.all()])
      return simulateLatency(created)
    },
  }
}

export function createApiNotificationService(api: ApiClient): NotificationService {
  return {
    listForUser: () => api.get<AppNotification[]>('/notifications'),
    listAll: () => api.get<AppNotification[]>('/admin/notifications'),
    markAsRead: async (id) => { await api.put<void>(`/notifications/${encodeURIComponent(id)}/read`) },
    markAllAsRead: async () => { await api.put<void>('/notifications/read-all') },
    remove: async (id) => { await api.delete<void>(`/notifications/${encodeURIComponent(id)}`) },
    create: request => api.post<AppNotification[]>('/admin/notifications', request),
  }
}
