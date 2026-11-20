<script setup lang="ts">
import type { NavItem } from '~/types'

const { user, logout } = useAuth()
const notifications = useNotifications()
const sidebarOpen = ref(false)

const items = computed<NavItem[]>(() => [
  { label: 'Dashboard', to: '/dashboard', icon: 'grid' },
  { label: 'Notifications', to: '/notifications', icon: 'bell', badge: notifications.unreadCount.value || undefined },
  { label: 'My Profile', to: '/profile', icon: 'user' },
  { label: 'Browse Ornaments', to: '/ornaments', icon: 'gem' },
])
</script>

<template>
  <div class="flex min-h-screen flex-col bg-cream/60">
    <AppNavbar />
    <div class="container-page flex flex-1 gap-8 py-8">
      <AppSidebar
        :items="items"
        title="My Account"
        :subtitle="user ? `Hello, ${user.fullName.split(' ')[0]}` : ''"
        :open="sidebarOpen"
        @close="sidebarOpen = false"
        @logout="logout"
      />
      <main class="min-w-0 flex-1">
        <button type="button" class="btn btn-outline btn-sm mb-6 lg:hidden" @click="sidebarOpen = true">
          <AppIcon name="menu" :size="16" /> Account menu
        </button>
        <slot />
      </main>
    </div>
    <AppFooter />
  </div>
</template>
