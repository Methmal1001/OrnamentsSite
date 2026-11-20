<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'My Dashboard' })

const { user } = useAuth()
const notifications = useNotifications()
const ornaments = useOrnaments()
const recentlyViewed = useRecentlyViewed()

notifications.load(true)
ornaments.load()

const viewed = computed(() =>
  recentlyViewed.ids.value
    .map(id => ornaments.items.value.find(o => o.id === id))
    .filter(o => o !== undefined)
    .slice(0, 4),
)

const greeting = computed(() => {
  const hour = new Date().getHours()
  return hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'
})
</script>

<template>
  <div v-if="user" class="space-y-8">
    <!-- Welcome -->
    <section class="relative overflow-hidden rounded-3xl bg-charcoal p-6 text-white sm:p-10">
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_90%_10%,rgb(195_154_82/0.45),transparent_55%)]" />
      <div class="relative flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p class="text-sm tracking-wide text-gold-200">{{ greeting }},</p>
          <h1 class="mt-1 font-serif text-4xl font-semibold sm:text-5xl">{{ user.fullName }}</h1>
          <p class="mt-2 max-w-md text-sm text-gold-100/75">Welcome to your Lanka Ornaments account. Here’s what’s new since your last visit.</p>
        </div>
        <NuxtLink to="/ornaments" class="btn btn-primary self-start sm:self-auto">Explore new arrivals <AppIcon name="arrow-right" :size="16" /></NuxtLink>
      </div>
    </section>

    <!-- Stats -->
    <section class="grid gap-4 sm:grid-cols-3">
      <StatCard label="Total notifications" :value="notifications.total.value" icon="bell" to="/notifications" />
      <StatCard label="Unread notifications" :value="notifications.unreadCount.value" icon="mail" hint="Tap to review" to="/notifications?filter=unread" />
      <StatCard label="Recently viewed" :value="viewed.length" icon="eye" hint="Ornaments you opened" />
    </section>

    <div class="grid gap-8 xl:grid-cols-[1fr_22rem]">
      <!-- Recent notifications -->
      <section class="card p-6">
        <div class="mb-5 flex items-center justify-between">
          <h2 class="text-2xl font-semibold">Recent notifications</h2>
          <NuxtLink to="/notifications" class="btn btn-ghost btn-sm">View all</NuxtLink>
        </div>
        <LoadingIndicator v-if="notifications.loading.value && !notifications.total.value" label="Loading notifications…" />
        <EmptyState v-else-if="notifications.error.value && !notifications.total.value" icon="warning" title="Unable to load notifications" :description="notifications.error.value">
          <button type="button" class="btn btn-primary" @click="notifications.load(true)">Try again</button>
        </EmptyState>
        <EmptyState v-else-if="!notifications.recent.value.length" icon="bell" title="No notifications yet" description="We’ll let you know about offers and updates here." />
        <div v-else class="space-y-3">
          <NotificationCard v-for="item in notifications.recent.value" :key="item.id" :notification="item" compact />
        </div>
      </section>

      <!-- Profile summary -->
      <section class="card h-fit p-6">
        <div class="flex items-center gap-4">
          <span class="grid size-14 place-items-center rounded-full bg-linear-to-br from-gold-300 to-gold-600 font-serif text-xl font-semibold text-white">{{ initials(user.fullName) }}</span>
          <div class="min-w-0">
            <h2 class="truncate text-xl font-semibold">{{ user.fullName }}</h2>
            <p class="text-xs text-charcoal-soft">Member since {{ formatDate(user.createdAt) }}</p>
          </div>
        </div>
        <dl class="mt-6 space-y-4 text-sm">
          <div class="flex gap-3"><AppIcon name="mail" :size="18" class="text-gold-600" /><dd class="min-w-0 break-words">{{ user.email }}</dd></div>
          <div class="flex gap-3"><AppIcon name="phone" :size="18" class="text-gold-600" /><dd>{{ user.phone }}</dd></div>
          <div class="flex gap-3"><AppIcon name="map-pin" :size="18" class="text-gold-600" /><dd>{{ user.address }}</dd></div>
        </dl>
        <NuxtLink to="/profile" class="btn btn-outline mt-6 w-full"><AppIcon name="edit" :size="16" /> Edit profile</NuxtLink>
      </section>
    </div>

    <!-- Recently viewed -->
    <section>
      <div class="mb-5 flex items-center justify-between">
        <h2 class="text-2xl font-semibold">Recently viewed ornaments</h2>
        <button v-if="viewed.length" type="button" class="btn btn-ghost btn-sm" @click="recentlyViewed.clear()">Clear history</button>
      </div>
      <LoadingIndicator v-if="ornaments.loading.value && !ornaments.items.value.length" />
      <EmptyState v-else-if="!viewed.length" icon="eye" title="Nothing viewed yet" description="Ornaments you open will appear here so you can find them again quickly.">
        <NuxtLink to="/ornaments" class="btn btn-primary">Browse ornaments</NuxtLink>
      </EmptyState>
      <div v-else class="grid gap-6 sm:grid-cols-2 2xl:grid-cols-4">
        <ProductCard v-for="item in viewed" :key="item.id" :ornament="item" />
      </div>
    </section>
  </div>
</template>
