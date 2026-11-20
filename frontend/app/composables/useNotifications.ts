import type { AppNotification, NotificationPreferences } from '~/types'

/** Notifications and notification preferences for the signed-in customer. */
export function useNotifications() {
  const { notifications: service, auth } = useServices()
  const { user } = useAuth()
  const toast = useToast()

  const items = useState<AppNotification[]>('notifications', () => [])
  const owner = useState<string | null>('notifications-owner', () => null)
  const loading = useState('notifications-loading', () => false)
  const error = useState<string | null>('notifications-error', () => null)

  const preferences = useState<NotificationPreferences | null>('notification-preferences', () => null)
  const preferencesSaving = useState('notification-preferences-saving', () => false)

  async function load(force = false) {
    const userId = user.value?.id
    if (!userId) {
      items.value = []
      owner.value = null
      return
    }
    if (!force && owner.value === userId) return
    loading.value = true
    error.value = null
    try {
      items.value = await service.listForUser(userId)
      owner.value = userId
    }
    catch (e) {
      error.value = errorMessage(e, 'Could not load your notifications.')
    }
    finally {
      loading.value = false
    }
  }

  const total = computed(() => items.value.length)
  const unreadCount = computed(() => items.value.filter(n => !n.isRead).length)
  const recent = computed(() => items.value.slice(0, 4))

  /** Applies a change immediately and rolls it back if the API call fails. */
  async function optimistic(change: (list: AppNotification[]) => AppNotification[], call: () => Promise<void>, failure: string) {
    const previous = items.value
    items.value = change(previous)
    try {
      await call()
      return true
    }
    catch (e) {
      items.value = previous
      toast.error(errorMessage(e, failure))
      return false
    }
  }

  const markAsRead = (id: string) => optimistic(
    list => list.map(n => (n.id === id ? { ...n, isRead: true } : n)),
    () => service.markAsRead(id),
    'Could not mark the notification as read.',
  )

  const markAllAsRead = () => optimistic(
    list => list.map(n => ({ ...n, isRead: true })),
    () => service.markAllAsRead(user.value?.id ?? ''),
    'Could not mark notifications as read.',
  )

  const remove = (id: string) => optimistic(
    list => list.filter(n => n.id !== id),
    () => service.remove(id),
    'Could not delete the notification.',
  )

  async function loadPreferences() {
    if (!user.value) return
    try {
      preferences.value = await auth.getPreferences(user.value.id)
    }
    catch (e) {
      toast.error(errorMessage(e, 'Could not load notification preferences.'))
    }
  }

  async function updatePreferences(next: NotificationPreferences) {
    if (!user.value) return false
    const previous = preferences.value
    preferences.value = next
    preferencesSaving.value = true
    try {
      preferences.value = await auth.updatePreferences(user.value.id, next)
      toast.success('Notification preferences saved.')
      return true
    }
    catch (e) {
      preferences.value = previous
      toast.error(errorMessage(e, 'Could not save notification preferences.'))
      return false
    }
    finally {
      preferencesSaving.value = false
    }
  }

  return {
    items, loading, error, total, unreadCount, recent, load, markAsRead, markAllAsRead, remove,
    preferences, preferencesSaving, loadPreferences, updatePreferences,
  }
}
