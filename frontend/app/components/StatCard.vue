<script setup lang="ts">
import type { IconName } from '~/types'

withDefaults(defineProps<{
  label: string
  value: string | number
  icon: IconName
  hint?: string
  to?: string
}>(), { hint: '', to: undefined })
</script>

<template>
  <component
    :is="to ? resolveComponent('NuxtLink') : 'div'"
    :to="to"
    class="card group flex items-center gap-4 p-5 transition-all duration-300"
    :class="to ? 'hover:-translate-y-0.5 hover:shadow-lift' : ''"
  >
    <span class="grid size-12 shrink-0 place-items-center rounded-2xl bg-linear-to-br from-gold-100 to-gold-200 text-gold-700 transition-transform duration-300 group-hover:scale-105">
      <AppIcon :name="icon" :size="22" />
    </span>
    <div class="min-w-0">
      <p class="text-xs font-medium uppercase tracking-wider text-charcoal-soft">{{ label }}</p>
      <p class="mt-0.5 truncate font-serif text-3xl font-semibold text-charcoal">{{ value }}</p>
      <p v-if="hint" class="truncate text-xs text-charcoal-soft">{{ hint }}</p>
    </div>
  </component>
</template>
