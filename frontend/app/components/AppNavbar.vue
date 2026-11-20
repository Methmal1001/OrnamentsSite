<script setup lang="ts">
const { user, isAuthenticated, isAdmin, homeRoute, logout } = useAuth()
const notifications = useNotifications()
const route = useRoute()

const links = [
  { label: 'Home', to: '/' },
  { label: 'Ornaments', to: '/ornaments' },
  { label: 'About Us', to: '/about' },
  { label: 'Contact', to: '/contact' },
]

const mobileOpen = ref(false)
const menuOpen = ref(false)
const menuRef = ref<HTMLElement | null>(null)

const isActive = (to: string) => (to === '/' ? route.path === '/' : route.path.startsWith(to))

watch(() => route.fullPath, () => {
  mobileOpen.value = false
  menuOpen.value = false
})

// Keep the unread badge up to date for signed-in customers.
watch(() => user.value?.id, (id) => {
  if (id && !isAdmin.value) notifications.load()
}, { immediate: true })

function onDocumentClick(event: MouseEvent) {
  if (menuRef.value && !menuRef.value.contains(event.target as Node)) menuOpen.value = false
}
onMounted(() => document.addEventListener('click', onDocumentClick))
onBeforeUnmount(() => document.removeEventListener('click', onDocumentClick))

async function handleLogout() {
  menuOpen.value = false
  mobileOpen.value = false
  await logout()
}
</script>

<template>
  <header class="sticky top-0 z-40 border-b border-gold-100 bg-white/85 backdrop-blur-md">
    <div class="hidden bg-charcoal text-center text-xs tracking-wide text-gold-100 sm:block">
      <p class="container-page py-2">
        Free island-wide insured delivery on orders over Rs. 150,000 · Lifetime cleaning &amp; polishing
      </p>
    </div>

    <nav class="container-page flex h-18 items-center justify-between gap-4" aria-label="Main">
      <AppLogo />

      <ul class="hidden items-center gap-1 lg:flex">
        <li v-for="link in links" :key="link.to">
          <NuxtLink
            :to="link.to"
            class="relative rounded-full px-4 py-2 text-sm font-medium transition-colors"
            :class="isActive(link.to) ? 'text-gold-700' : 'text-charcoal-soft hover:text-gold-700'"
          >
            {{ link.label }}
            <span
              class="absolute inset-x-4 -bottom-0.5 h-px origin-left bg-gold-500 transition-transform duration-300"
              :class="isActive(link.to) ? 'scale-x-100' : 'scale-x-0'"
            />
          </NuxtLink>
        </li>
      </ul>

      <div class="flex items-center gap-2">
        <template v-if="isAuthenticated && user">
          <NuxtLink
            v-if="!isAdmin"
            to="/notifications"
            class="relative grid size-10 place-items-center rounded-full text-charcoal-soft transition hover:bg-gold-50 hover:text-gold-700"
            :aria-label="`Notifications (${notifications.unreadCount.value} unread)`"
          >
            <AppIcon name="bell" :size="22" />
            <span
              v-if="notifications.unreadCount.value > 0"
              class="absolute right-1 top-1 grid min-w-4.5 place-items-center rounded-full bg-gold-600 px-1 text-[0.65rem] font-semibold text-white"
            >
              {{ notifications.unreadCount.value }}
            </span>
          </NuxtLink>

          <div ref="menuRef" class="relative hidden sm:block">
            <button
              type="button"
              class="flex cursor-pointer items-center gap-2 rounded-full border border-gold-200 py-1 pl-1 pr-3 transition hover:border-gold-400"
              :aria-expanded="menuOpen"
              @click="menuOpen = !menuOpen"
            >
              <span class="grid size-8 place-items-center rounded-full bg-gold-100 text-xs font-semibold text-gold-800">
                {{ initials(user.fullName) }}
              </span>
              <span class="max-w-28 truncate text-sm font-medium">{{ user.fullName.split(' ')[0] }}</span>
              <AppIcon name="chevron-down" :size="14" />
            </button>

            <Transition
              enter-active-class="transition duration-150 ease-out"
              enter-from-class="opacity-0 -translate-y-1"
              leave-active-class="transition duration-100 ease-in"
              leave-to-class="opacity-0 -translate-y-1"
            >
              <div v-if="menuOpen" class="card absolute right-0 mt-2 w-56 overflow-hidden p-1.5">
                <p class="px-3 py-2 text-xs text-charcoal-soft">
                  Signed in as<br><span class="font-medium text-charcoal">{{ user.email }}</span>
                </p>
                <NuxtLink :to="homeRoute" class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm hover:bg-gold-50">
                  <AppIcon :name="isAdmin ? 'chart' : 'grid'" :size="16" /> {{ isAdmin ? 'Admin console' : 'My dashboard' }}
                </NuxtLink>
                <NuxtLink v-if="!isAdmin" to="/profile" class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm hover:bg-gold-50">
                  <AppIcon name="user" :size="16" /> My profile
                </NuxtLink>
                <button type="button" class="flex w-full cursor-pointer items-center gap-2 rounded-lg px-3 py-2 text-sm text-rose-600 hover:bg-rose-50" @click="handleLogout">
                  <AppIcon name="logout" :size="16" /> Log out
                </button>
              </div>
            </Transition>
          </div>
        </template>

        <template v-else>
          <NuxtLink to="/login" class="btn btn-ghost hidden sm:inline-flex">Login</NuxtLink>
          <NuxtLink to="/register" class="btn btn-primary hidden sm:inline-flex">Register</NuxtLink>
        </template>

        <button
          type="button"
          class="grid size-10 cursor-pointer place-items-center rounded-full text-charcoal transition hover:bg-gold-50 lg:hidden"
          :aria-label="mobileOpen ? 'Close menu' : 'Open menu'"
          :aria-expanded="mobileOpen"
          @click="mobileOpen = !mobileOpen"
        >
          <AppIcon :name="mobileOpen ? 'close' : 'menu'" :size="24" />
        </button>
      </div>
    </nav>

    <Transition
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="opacity-0 -translate-y-2"
      leave-active-class="transition duration-150 ease-in"
      leave-to-class="opacity-0 -translate-y-2"
    >
      <div v-if="mobileOpen" class="border-t border-gold-100 bg-white lg:hidden">
        <div class="container-page space-y-1 py-4">
          <NuxtLink
            v-for="link in links"
            :key="link.to"
            :to="link.to"
            class="block rounded-xl px-4 py-3 text-sm font-medium"
            :class="isActive(link.to) ? 'bg-gold-50 text-gold-700' : 'text-charcoal hover:bg-gold-50'"
          >
            {{ link.label }}
          </NuxtLink>

          <div class="mt-3 border-t border-gold-100 pt-4">
            <template v-if="isAuthenticated">
              <NuxtLink :to="homeRoute" class="block rounded-xl px-4 py-3 text-sm font-medium hover:bg-gold-50">
                {{ isAdmin ? 'Admin console' : 'My dashboard' }}
              </NuxtLink>
              <NuxtLink v-if="!isAdmin" to="/profile" class="block rounded-xl px-4 py-3 text-sm font-medium hover:bg-gold-50">
                My profile
              </NuxtLink>
              <button type="button" class="block w-full cursor-pointer rounded-xl px-4 py-3 text-left text-sm font-medium text-rose-600 hover:bg-rose-50" @click="handleLogout">
                Log out
              </button>
            </template>
            <div v-else class="grid grid-cols-2 gap-3">
              <NuxtLink to="/login" class="btn btn-outline">Login</NuxtLink>
              <NuxtLink to="/register" class="btn btn-primary">Register</NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </header>
</template>
