<script setup lang="ts">
import type { NotificationFilter } from '~/types'

definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'Notifications' })

const route = useRoute()
const toast = useToast()
const {
  items, loading, error, total, unreadCount, load, markAsRead, markAllAsRead, remove,
  preferences, preferencesSaving, loadPreferences, updatePreferences,
} = useNotifications()
// Always fetch fresh data from the server when the page opens.
load(true)
loadPreferences()

function togglePreference(key: 'emailNotificationsEnabled' | 'inAppNotificationsEnabled', value: boolean) {
  if (!preferences.value) return
  updatePreferences({ ...preferences.value, [key]: value })
}

const filters: { value: NotificationFilter, label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'unread', label: 'Unread' },
  { value: 'read', label: 'Read' },
]

const filterFromQuery = (): NotificationFilter =>
  filters.find(f => f.value === route.query.filter)?.value ?? 'all'

const filter = ref<NotificationFilter>(filterFromQuery())
watch(() => route.query.filter, () => { filter.value = filterFromQuery() })

const counts = computed<Record<NotificationFilter, number>>(() => ({
  all: total.value,
  unread: unreadCount.value,
  read: total.value - unreadCount.value,
}))

const visible = computed(() => items.value.filter(n =>
  filter.value === 'all' || (filter.value === 'unread' ? !n.isRead : n.isRead),
))

const preferenceOptions = [
  { key: 'inAppNotificationsEnabled', label: 'In-app notifications', hint: 'Offers and order updates on this page and in the navigation bell.' },
  { key: 'emailNotificationsEnabled', label: 'Email notifications', hint: 'Receive updates by email (sending starts in a later release).' },
] as const

const pendingDelete = ref<string | null>(null)
const deleting = ref(false)

async function confirmDelete() {
  if (!pendingDelete.value) return
  deleting.value = true
  try {
    if (await remove(pendingDelete.value)) toast.success('Notification deleted.')
  }
  finally {
    deleting.value = false
    pendingDelete.value = null
  }
}

async function handleMarkAll() {
  if (await markAllAsRead()) toast.success('All notifications marked as read.')
}
</script>

<template>
  <div class="space-y-6">
    <header class="flex flex-wrap items-end justify-between gap-4">
      <div>
        <p class="eyebrow">Stay up to date</p>
        <h1 class="section-title mt-1">Notifications</h1>
        <p class="mt-1 text-sm text-charcoal-soft">{{ unreadCount }} unread of {{ total }} total</p>
      </div>
      <button type="button" class="btn btn-primary" :disabled="unreadCount === 0" @click="handleMarkAll">
        <AppIcon name="check" :size="16" /> Mark all as read
      </button>
    </header>

    <div class="inline-flex rounded-full border border-gold-200 bg-white p-1" role="tablist" aria-label="Filter notifications">
      <button
        v-for="option in filters"
        :key="option.value"
        type="button"
        role="tab"
        :aria-selected="filter === option.value"
        class="cursor-pointer rounded-full px-4 py-1.5 text-sm font-medium transition"
        :class="filter === option.value ? 'bg-gold-500 text-white shadow-soft' : 'text-charcoal-soft hover:text-gold-700'"
        @click="filter = option.value"
      >
        {{ option.label }}
        <span class="ml-1 text-xs opacity-80">({{ counts[option.value] }})</span>
      </button>
    </div>

    <LoadingIndicator v-if="loading && !total" label="Loading notifications…" />
    <EmptyState v-else-if="error && !total" icon="warning" title="Unable to load notifications" :description="error">
      <button type="button" class="btn btn-primary" @click="load(true)">Try again</button>
    </EmptyState>
    <EmptyState
      v-else-if="!visible.length"
      icon="bell"
      :title="filter === 'unread' ? 'You’re all caught up' : 'No notifications'"
      :description="filter === 'all' ? 'New offers and account updates will appear here.' : `There are no ${filter} notifications.`"
    >
      <button v-if="filter !== 'all'" type="button" class="btn btn-outline" @click="filter = 'all'">Show all</button>
    </EmptyState>
    <TransitionGroup
      v-else
      tag="div"
      class="space-y-3"
      leave-active-class="transition duration-200"
      leave-to-class="opacity-0 -translate-x-4"
    >
      <NotificationCard
        v-for="item in visible"
        :key="item.id"
        :notification="item"
        @mark-read="markAsRead"
        @delete="pendingDelete = $event"
      />
    </TransitionGroup>

    <!-- Preferences -->
    <section class="card p-6" aria-labelledby="preferences-heading">
      <div class="mb-5 flex items-start gap-4">
        <span class="grid size-11 shrink-0 place-items-center rounded-full bg-gold-50 text-gold-600"><AppIcon name="bell" /></span>
        <div>
          <h2 id="preferences-heading" class="text-2xl font-semibold">Notification preferences</h2>
          <p class="text-sm text-charcoal-soft">Choose how we keep in touch. Account and security messages are always delivered.</p>
        </div>
      </div>
      <LoadingIndicator v-if="!preferences" variant="inline" label="Loading preferences" />
      <div v-else class="grid gap-3 sm:grid-cols-2">
        <label
          v-for="option in preferenceOptions"
          :key="option.key"
          class="flex cursor-pointer items-start gap-3 rounded-2xl border border-gold-100 p-4 transition hover:border-gold-300"
          :class="{ 'pointer-events-none opacity-70': preferencesSaving }"
        >
          <input
            type="checkbox"
            class="peer sr-only"
            :checked="preferences[option.key]"
            :disabled="preferencesSaving"
            @change="togglePreference(option.key, ($event.target as HTMLInputElement).checked)"
          >
          <span
            class="relative mt-0.5 h-6 w-11 shrink-0 rounded-full bg-beige transition peer-checked:bg-gold-500 peer-focus-visible:ring-4 peer-focus-visible:ring-gold-100 after:absolute after:left-0.5 after:top-0.5 after:size-5 after:rounded-full after:bg-white after:shadow-soft after:transition peer-checked:after:translate-x-5"
            aria-hidden="true"
          />
          <span>
            <span class="block text-sm font-semibold">{{ option.label }}</span>
            <span class="block text-xs text-charcoal-soft">{{ option.hint }}</span>
          </span>
        </label>
      </div>
    </section>

    <ConfirmDialog
      :open="pendingDelete !== null"
      title="Delete notification"
      message="This notification will be permanently removed from your account."
      confirm-label="Delete"
      :loading="deleting"
      @confirm="confirmDelete"
      @cancel="pendingDelete = null"
    />
  </div>
</template>
