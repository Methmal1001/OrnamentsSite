<script setup lang="ts">
import type { NavItem } from '~/types'

const { user, logout } = useAuth()
const sidebarOpen = ref(false)
const mockMode = useRuntimeConfig().public.useMockApi

const items: NavItem[] = [
  { label: 'Dashboard', to: '/admin', icon: 'chart' },
  { label: 'Ornaments', to: '/admin/ornaments', icon: 'gem' },
  { label: 'Notifications', to: '/admin/notifications', icon: 'bell' },
]
</script>

<template>
  <div class="flex min-h-screen bg-beige/50">
    <AppSidebar
      :items="items"
      title="Admin Console"
      subtitle="Lanka Ornaments"
      variant="dark"
      :open="sidebarOpen"
     
      @close="sidebarOpen = false"
      @logout="logout"
    >
      <template #footer>
        <NuxtLink to="/" class="mb-1 flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium text-gold-100/70 transition hover:bg-white/5 hover:text-white">
          <AppIcon name="home" /> View storefront
        </NuxtLink>
      </template>
    </AppSidebar>

    <div class="flex min-w-0 flex-1 flex-col">
      <header class="sticky top-0 z-30 flex h-16 items-center justify-between gap-4 border-b border-gold-100 bg-white/90 px-4 backdrop-blur sm:px-8">
        <div class="flex items-center gap-3">
          <button type="button" class="grid size-10 cursor-pointer place-items-center rounded-full hover:bg-gold-50 lg:hidden" aria-label="Open admin menu" @click="sidebarOpen = true">
            <AppIcon name="menu" :size="22" />
          </button>
          <span v-if="mockMode" class="badge bg-amber-50 text-amber-700 ring-1 ring-amber-200">
            <AppIcon name="shield" :size="12" /> Simulated admin access
          </span>
          <span v-else class="badge bg-gold-50 text-gold-800 ring-1 ring-gold-200">
            <AppIcon name="shield" :size="12" /> Administrator
          </span>
        </div>
        <div v-if="user" class="flex items-center gap-3">
          <div class="hidden text-right sm:block">
            <p class="text-sm font-medium">{{ user.fullName }}</p>
            <p class="text-xs text-charcoal-soft">{{ user.email }}</p>
          </div>
          <span class="grid size-10 place-items-center rounded-full bg-charcoal text-sm font-semibold text-gold-200">{{ initials(user.fullName) }}</span>
        </div>
      </header>
      <main class="flex-1 p-4 sm:p-8">
        <slot />
      </main>
    </div>
  </div>
</template>
