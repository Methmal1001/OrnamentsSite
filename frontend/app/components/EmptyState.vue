<script setup lang="ts">
import type { IconName } from '~/types'

withDefaults(defineProps<{ title: string, description?: string, icon?: IconName }>(), {
  description: '',
  icon: 'inbox',
})
</script>

<template>
  <div class="flex flex-col items-center rounded-2xl border border-dashed border-gold-200 bg-white/60 px-6 py-14 text-center">
    <span class="grid size-16 place-items-center rounded-full bg-gold-50 text-gold-600">
      <AppIcon :name="icon" :size="28" />
    </span>
    <h3 class="mt-5 font-serif text-2xl font-semibold">{{ title }}</h3>
    <p v-if="description" class="mt-2 max-w-md text-sm text-charcoal-soft">{{ description }}</p>
    <div v-if="$slots.default" class="mt-6 flex flex-wrap justify-center gap-3">
      <slot />
    </div>
  </div>
</template>
