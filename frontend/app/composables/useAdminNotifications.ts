import type { AppNotification, CreateNotificationRequest, User } from '~/types'

/** Notification management for the (simulated) administrator. */
export function useAdminNotifications() {
  const { notifications: service, auth } = useServices()

  const items = useState<AppNotification[]>('admin-notifications', () => [])
  const customers = useState<User[]>('admin-customers', () => [])
  const loading = useState('admin-notifications-loading', () => false)
  const error = useState<string | null>('admin-notifications-error', () => null)

  async function load() {
    loading.value = true
    error.value = null
    try {
      const [all, customerList] = await Promise.all([service.listAll(), auth.listCustomers()])
      items.value = all
      customers.value = customerList
    }
    catch (e) {
      error.value = errorMessage(e, 'Could not load notifications.')
    }
    finally {
      loading.value = false
    }
  }

  async function create(request: CreateNotificationRequest) {
    const recipientIds = request.recipient === 'all' ? customers.value.map(c => c.id) : [request.recipient]
    if (recipientIds.length === 0) throw new Error('There are no customers to notify')
    const created = await service.create(request, recipientIds)
    items.value = [...created, ...items.value]
    return created
  }

  async function remove(id: string) {
    await service.remove(id)
    items.value = items.value.filter(n => n.id !== id)
  }

  function customerName(userId: string | null) {
    if (userId === null) return 'All customers (shop-wide)'
    return customers.value.find(c => c.id === userId)?.fullName ?? 'Unknown customer'
  }

  const unreadCount = computed(() => items.value.filter(n => !n.isRead).length)

  return { items, customers, loading, error, unreadCount, load, create, remove, customerName }
}
