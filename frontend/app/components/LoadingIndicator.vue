<script setup lang="ts">
withDefaults(defineProps<{
  label?: string
  /** `block` centres a spinner with label; `inline` is a small spinner for buttons. */
  variant?: 'block' | 'inline'
}>(), { label: 'Loading…', variant: 'block' })
</script>

<template>
  <span v-if="variant === 'inline'" class="inline-block size-4 animate-spin rounded-full border-2 border-current border-r-transparent" role="status" :aria-label="label" />
  <div v-else class="flex flex-col items-center justify-center gap-4 py-16" role="status">
    <div class="relative size-12">
      <span class="absolute inset-0 rounded-full border-2 border-gold-100" />
      <span class="absolute inset-0 animate-spin rounded-full border-2 border-gold-500 border-t-transparent" />
      <AppIcon name="gem" :size="18" class="absolute inset-0 m-auto text-gold-500" />
    </div>
    <p class="text-sm tracking-wide text-charcoal-soft">{{ label }}</p>
  </div>
</template>
