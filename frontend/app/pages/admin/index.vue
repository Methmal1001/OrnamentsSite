<script setup lang="ts">
import { categories } from '~/data/categories'

definePageMeta({ layout: 'admin', middleware: 'admin' })
useHead({ title: 'Admin Dashboard' })

const ornaments = useOrnaments()
const admin = useAdminNotifications()
ornaments.load(true)
admin.load()

const stats = computed(() => {
  const list = ornaments.items.value
  return {
    total: list.length,
    inStock: list.filter(o => o.availability === 'in-stock').length,
    attention: list.filter(o => o.availability === 'low-stock' || o.availability === 'out-of-stock').length,
    value: list.reduce((sum, o) => sum + o.price, 0),
  }
})

const breakdown = computed(() => {
  const max = Math.max(1, ...Object.values(ornaments.countByCategory.value))
  return categories.map(c => ({
    ...c,
    count: ornaments.countByCategory.value[c.id],
    percent: Math.round((ornaments.countByCategory.value[c.id] / max) * 100),
  }))
})

const recent = computed(() => ornaments.latest.value.slice(0, 5))
</script>

<template>
  <div class="space-y-8">
    <header class="flex flex-wrap items-end justify-between gap-4">
      <div>
        <p class="eyebrow">Overview</p>
        <h1 class="section-title mt-1">Dashboard</h1>
      </div>
      <div class="flex flex-wrap gap-2">
        <NuxtLink to="/admin/notifications" class="btn btn-outline"><AppIcon name="bell" :size="16" /> Send notification</NuxtLink>
        <NuxtLink :to="{ path: '/admin/ornaments', query: { new: '1' } }" class="btn btn-primary"><AppIcon name="plus" :size="16" /> Add ornament</NuxtLink>
      </div>
    </header>

    <section class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <StatCard label="Total ornaments" :value="stats.total" icon="gem" :hint="`${stats.inStock} in stock`" to="/admin/ornaments" />
      <StatCard label="Need attention" :value="stats.attention" icon="warning" hint="Low or out of stock" to="/admin/ornaments" />
      <StatCard label="Customers" :value="admin.customers.value.length" icon="users" hint="Registered accounts" />
      <StatCard label="Notifications" :value="admin.items.value.length" icon="bell" :hint="`${admin.unreadCount.value} unread by customers`" to="/admin/notifications" />
    </section>

    <div class="grid gap-8 xl:grid-cols-[1fr_24rem]">
      <section class="card overflow-hidden">
        <div class="flex items-center justify-between border-b border-gold-100 px-6 py-4">
          <h2 class="text-2xl font-semibold">Latest ornaments</h2>
          <NuxtLink to="/admin/ornaments" class="btn btn-ghost btn-sm">Manage</NuxtLink>
        </div>
        <LoadingIndicator v-if="ornaments.loading.value && !recent.length" />
        <p v-else-if="ornaments.error.value" class="flex items-center justify-between gap-3 px-6 py-5 text-sm text-rose-700">
          {{ ornaments.error.value }}
          <button type="button" class="btn btn-outline btn-sm" @click="ornaments.load(true)">Retry</button>
        </p>
        <ul v-else class="divide-y divide-gold-100">
          <li v-for="item in recent" :key="item.id" class="flex items-center gap-4 px-6 py-3">
            <OrnamentImage :ornament="item" class="size-12 shrink-0 rounded-xl" />
            <div class="min-w-0 flex-1">
              <p class="truncate font-medium">{{ item.name }}</p>
              <p class="text-xs text-charcoal-soft">{{ categoryName(item.categoryId) }} · Added {{ formatDate(item.createdAt) }}</p>
            </div>
            <span class="badge hidden sm:inline-flex" :class="availabilityMeta[item.availability].classes">{{ availabilityMeta[item.availability].label }}</span>
            <span class="whitespace-nowrap text-sm font-semibold text-gold-700">{{ formatPrice(item.price) }}</span>
          </li>
        </ul>
      </section>

      <section class="card h-fit p-6">
        <h2 class="text-2xl font-semibold">Catalogue by category</h2>
        <p class="text-xs text-charcoal-soft">Listed value: {{ formatPrice(stats.value) }}</p>
        <ul class="mt-6 space-y-4">
          <li v-for="row in breakdown" :key="row.id">
            <div class="mb-1 flex justify-between text-sm">
              <span>{{ row.name }}</span>
              <span class="font-medium">{{ row.count }}</span>
            </div>
            <div class="h-2 overflow-hidden rounded-full bg-gold-50">
              <div class="h-full rounded-full bg-linear-to-r from-gold-400 to-gold-600 transition-all duration-700" :style="{ width: `${row.percent}%` }" />
            </div>
          </li>
        </ul>
      </section>
    </div>
  </div>
</template>
