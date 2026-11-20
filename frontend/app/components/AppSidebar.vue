<script setup lang="ts">
import type { NavItem } from '~/types'

const props = withDefaults(defineProps<{
  items: NavItem[]
  title: string
  subtitle?: string
  /** Mobile drawer state; the sidebar is always visible from `lg` up. */
  open?: boolean
  variant?: 'light' | 'dark'
}>(), { subtitle: '', open: false, variant: 'light' })

const emit = defineEmits<{ close: [], logout: [] }>()
const route = useRoute()

const isActive = (to: string) => route.path === to || (to !== '/admin' && to !== '/dashboard' && route.path.startsWith(`${to}/`))

const dark = computed(() => props.variant === 'dark')

watch(() => route.fullPath, () => emit('close'))
</script>

<template>
  <div class="contents">
  <!-- Backdrop for mobile drawer -->
  <Transition enter-active-class="transition-opacity duration-200" enter-from-class="opacity-0" leave-active-class="transition-opacity duration-150" leave-to-class="opacity-0">
    <div v-if="open" class="fixed inset-0 z-40 bg-charcoal/40 backdrop-blur-sm lg:hidden" @click="emit('close')" />
  </Transition>

  <aside
    class="fixed inset-y-0 left-0 z-50 flex w-72 flex-col transition-transform duration-300 lg:sticky lg:bottom-auto lg:z-auto lg:translate-x-0"
    :class="[
      open ? 'translate-x-0' : '-translate-x-full',
      dark
        ? 'bg-charcoal text-gold-100 lg:top-0 lg:h-screen'
        : 'border-r border-gold-100 bg-white lg:top-32 lg:self-start lg:rounded-2xl lg:border lg:shadow-soft',
    ]"
    aria-label="Sidebar"
  >
    <div class="flex items-start justify-between gap-2 p-6" :class="dark ? 'border-b border-white/10' : 'border-b border-gold-100'">
      <div>
        <p class="eyebrow" :class="dark ? 'text-gold-300' : ''">{{ subtitle }}</p>
        <p class="mt-1 font-serif text-xl font-semibold" :class="dark ? 'text-white' : 'text-charcoal'">{{ title }}</p>
      </div>
      <button type="button" class="cursor-pointer rounded-full p-1.5 hover:bg-gold-50/10 lg:hidden" aria-label="Close sidebar" @click="emit('close')">
        <AppIcon name="close" />
      </button>
    </div>

    <nav class="flex-1 space-y-1 p-4">
      <NuxtLink
        v-for="item in items"
        :key="item.to"
        :to="item.to"
        class="flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium transition-all duration-200"
        :class="isActive(item.to)
          ? (dark ? 'bg-gold-500 text-white shadow-soft' : 'bg-gold-50 text-gold-800 ring-1 ring-gold-200')
          : (dark ? 'text-gold-100/70 hover:bg-white/5 hover:text-white' : 'text-charcoal-soft hover:bg-gold-50 hover:text-gold-700')"
      >
        <AppIcon :name="item.icon" />
        <span class="flex-1">{{ item.label }}</span>
        <span v-if="item.badge" class="rounded-full bg-gold-600 px-2 py-0.5 text-xs font-semibold text-white">{{ item.badge }}</span>
      </NuxtLink>
    </nav>

    <div class="p-4" :class="dark ? 'border-t border-white/10' : 'border-t border-gold-100'">
      <slot name="footer" />
      <button
        type="button"
        class="flex w-full cursor-pointer items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium transition"
        :class="dark ? 'text-rose-300 hover:bg-white/5' : 'text-rose-600 hover:bg-rose-50'"
        @click="emit('logout')"
      >
        <AppIcon name="logout" /> Log out
      </button>
    </div>
  </aside>
  </div>
</template>
