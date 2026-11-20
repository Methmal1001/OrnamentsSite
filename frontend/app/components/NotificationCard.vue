<script setup lang="ts">
import type { AppNotification } from '~/types'

withDefaults(defineProps<{
  notification: AppNotification
  /** Compact cards hide the action buttons (used on the dashboard). */
  compact?: boolean
}>(), { compact: false })

const emit = defineEmits<{ markRead: [id: string], delete: [id: string] }>()
</script>

<template>
  <article
    class="group relative flex gap-4 rounded-2xl border p-4 transition-all duration-200 sm:p-5"
    :class="notification.isRead ? 'border-gold-100 bg-white' : 'border-gold-200 bg-gold-50/70 shadow-soft'"
  >
    <span
      class="grid size-10 shrink-0 place-items-center rounded-full"
      :class="notification.isRead ? 'bg-beige text-charcoal-soft' : 'bg-gold-500 text-white'"
    >
      <AppIcon :name="notification.type === 'promotion' ? 'gift' : notification.type === 'order' ? 'tag' : notification.type === 'account' ? 'user' : 'bell'" :size="18" />
    </span>

    <div class="min-w-0 flex-1">
      <div class="flex flex-wrap items-center gap-2">
        <h3 class="font-sans text-sm font-semibold text-charcoal sm:text-base">{{ notification.title }}</h3>
        <span class="badge" :class="notificationTypeMeta[notification.type].classes">{{ notificationTypeMeta[notification.type].label }}</span>
        <span v-if="!notification.isRead" class="badge bg-gold-600 text-white">New</span>
      </div>
      <p class="mt-1 text-sm leading-relaxed text-charcoal-soft" :class="compact ? 'line-clamp-1' : ''">{{ notification.message }}</p>
      <p class="mt-2 flex items-center gap-1.5 text-xs text-charcoal-soft/80">
        <AppIcon name="clock" :size="13" />
        <time :datetime="notification.createdAt" :title="formatDateTime(notification.createdAt)">
          {{ formatDateTime(notification.createdAt) }} · {{ formatRelativeTime(notification.createdAt) }}
        </time>
      </p>

      <div v-if="!compact" class="mt-3 flex flex-wrap gap-2">
        <button
          v-if="!notification.isRead"
          type="button"
          class="btn btn-outline btn-sm"
          @click="emit('markRead', notification.id)"
        >
          <AppIcon name="check" :size="14" /> Mark as read
        </button>
        <button type="button" class="btn btn-ghost btn-sm text-rose-600! hover:bg-rose-50!" @click="emit('delete', notification.id)">
          <AppIcon name="trash" :size="14" /> Delete
        </button>
      </div>
    </div>
  </article>
</template>
