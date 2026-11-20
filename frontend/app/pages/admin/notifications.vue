<script setup lang="ts">
import type { AppNotification, CreateNotificationRequest, NotificationType } from '~/types'

definePageMeta({ layout: 'admin', middleware: 'admin' })
useHead({ title: 'Manage Notifications' })

const toast = useToast()
const { items, customers, loading, error, load, create, remove, customerName } = useAdminNotifications()
load()

const types = Object.entries(notificationTypeMeta) as [NotificationType, { label: string }][]

const form = reactive<CreateNotificationRequest>({ recipient: 'all', title: '', message: '', type: 'promotion' })
const sending = ref(false)

const { errors, validate, validateField, clearErrors } = useFormValidation(form, {
  title: [rules.required('Title'), rules.maxLength(80, 'Title')],
  message: [rules.required('Message'), rules.minLength(10, 'Message'), rules.maxLength(500, 'Message')],
})

async function send() {
  if (!validate()) return
  sending.value = true
  try {
    const created = await create(form)
    toast.success(created.some(n => n.userId === null)
      ? 'Announcement published to all customers.'
      : `Notification sent to ${created.length} customer${created.length === 1 ? '' : 's'}.`)
    Object.assign(form, { title: '', message: '' })
    clearErrors()
  }
  catch (error) {
    toast.error(errorMessage(error))
  }
  finally {
    sending.value = false
  }
}

const search = ref('')
const visible = computed(() => {
  const term = search.value.trim().toLowerCase()
  return term
    ? items.value.filter(n => [n.title, n.message, customerName(n.userId)].some(f => f.toLowerCase().includes(term)))
    : items.value
})

const pendingDelete = ref<AppNotification | null>(null)
const deleting = ref(false)

async function confirmDelete() {
  if (!pendingDelete.value) return
  deleting.value = true
  try {
    await remove(pendingDelete.value.id)
    toast.success('Notification deleted.')
    pendingDelete.value = null
  }
  finally {
    deleting.value = false
  }
}
</script>

<template>
  <div class="space-y-6">
    <header>
      <p class="eyebrow">Customer messaging</p>
      <h1 class="section-title mt-1">Notification Management</h1>
    </header>

    <div class="grid gap-6 xl:grid-cols-[24rem_1fr]">
      <form class="card h-fit space-y-5 p-6" novalidate @submit.prevent="send">
        <h2 class="text-2xl font-semibold">Create notification</h2>

        <div>
          <label class="label" for="recipient">Send to</label>
          <select id="recipient" v-model="form.recipient" class="input">
            <option value="all">All customers ({{ customers.length }})</option>
            <option v-for="customer in customers" :key="customer.id" :value="customer.id">{{ customer.fullName }} – {{ customer.email }}</option>
          </select>
        </div>

        <div>
          <label class="label" for="type">Type</label>
          <select id="type" v-model="form.type" class="input">
            <option v-for="[value, meta] in types" :key="value" :value="value">{{ meta.label }}</option>
          </select>
        </div>

        <FormInput v-model="form.title" label="Title" required :error="errors.title" @blur="validateField('title')" />
        <FormInput v-model="form.message" label="Message" multiline :rows="4" required :error="errors.message" :hint="`${form.message.length}/500 characters`" @blur="validateField('message')" />

        <button type="submit" class="btn btn-primary w-full" :disabled="sending">
          <LoadingIndicator v-if="sending" variant="inline" />
          Send notification
        </button>
      </form>

      <section class="card overflow-hidden">
        <div class="flex flex-col gap-3 border-b border-gold-100 p-4 sm:flex-row sm:items-center sm:justify-between">
          <h2 class="px-2 text-2xl font-semibold">Sent notifications <span class="text-base font-normal text-charcoal-soft">({{ items.length }})</span></h2>
          <div class="sm:w-72"><SearchBar v-model="search" label="admin-notifications" placeholder="Search notifications…" /></div>
        </div>

        <LoadingIndicator v-if="loading && !items.length" label="Loading notifications…" />
        <div v-else-if="error && !items.length" class="p-6">
          <EmptyState icon="warning" title="Unable to load notifications" :description="error">
            <button type="button" class="btn btn-primary" @click="load">Try again</button>
          </EmptyState>
        </div>
        <div v-else-if="!visible.length" class="p-6">
          <EmptyState icon="bell" title="No notifications" description="Notifications you send to customers will be listed here." />
        </div>
        <ul v-else class="divide-y divide-gold-100">
          <li v-for="item in visible" :key="item.id" class="flex gap-4 p-4 sm:px-6">
            <div class="min-w-0 flex-1">
              <div class="flex flex-wrap items-center gap-2">
                <p class="font-medium">{{ item.title }}</p>
                <span class="badge" :class="notificationTypeMeta[item.type].classes">{{ notificationTypeMeta[item.type].label }}</span>
                <span class="badge" :class="item.isRead ? 'bg-emerald-50 text-emerald-700' : 'bg-beige text-charcoal-soft'">{{ item.isRead ? 'Read' : 'Unread' }}</span>
              </div>
              <p class="mt-1 line-clamp-2 text-sm text-charcoal-soft">{{ item.message }}</p>
              <p class="mt-1 text-xs text-charcoal-soft/80">
                To <span class="font-medium text-charcoal">{{ customerName(item.userId) }}</span> · {{ formatDateTime(item.createdAt) }}
              </p>
            </div>
            <button type="button" class="grid size-9 shrink-0 cursor-pointer place-items-center rounded-full text-charcoal-soft transition hover:bg-rose-50 hover:text-rose-600" :aria-label="`Delete ${item.title}`" @click="pendingDelete = item">
              <AppIcon name="trash" :size="18" />
            </button>
          </li>
        </ul>
      </section>
    </div>

    <ConfirmDialog
      :open="pendingDelete !== null"
      title="Delete notification"
      :message="`Remove “${pendingDelete?.title}” for ${pendingDelete ? customerName(pendingDelete.userId) : ''}? The customer will no longer see it.`"
      confirm-label="Delete"
      :loading="deleting"
      @confirm="confirmDelete"
      @cancel="pendingDelete = null"
    />
  </div>
</template>
