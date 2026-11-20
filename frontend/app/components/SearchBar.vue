<script setup lang="ts">
withDefaults(defineProps<{ placeholder?: string, label?: string }>(), {
  placeholder: 'Search…',
  label: 'Search',
})

const model = defineModel<string>({ default: '' })
</script>

<template>
  <div class="relative">
    <label class="sr-only" :for="`search-${label}`">{{ label }}</label>
    <AppIcon name="search" :size="18" class="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-gold-600" />
    <input
      :id="`search-${label}`"
      v-model="model"
      type="search"
      :placeholder="placeholder"
      class="input rounded-full! py-3 pl-11 pr-10 [&::-webkit-search-cancel-button]:hidden"
    >
    <button
      v-if="model"
      type="button"
      class="absolute right-3 top-1/2 grid size-7 -translate-y-1/2 cursor-pointer place-items-center rounded-full text-charcoal-soft transition hover:bg-gold-50 hover:text-charcoal"
      aria-label="Clear search"
      @click="model = ''"
    >
      <AppIcon name="close" :size="16" />
    </button>
  </div>
</template>
