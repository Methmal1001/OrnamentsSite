<script setup lang="ts">
withDefaults(defineProps<{ light?: boolean, to?: string }>(), { light: false, to: '/' })
</script>

<template>
  <NuxtLink :to="to" class="group inline-flex items-center gap-2.5" aria-label="Lanka Ornaments home">
    <span class="grid size-10 place-items-center rounded-full bg-linear-to-br from-gold-300 via-gold-500 to-gold-700 text-white shadow-soft transition-transform duration-500 group-hover:rotate-12">
      <AppIcon name="gem" :size="20" />
    </span>
    <span class="leading-none">
      <span class="block font-serif text-xl font-semibold tracking-wide" :class="light ? 'text-white' : 'text-charcoal'">
        Lanka Ornaments
      </span>
      <span class="mt-1 hidden whitespace-nowrap text-[0.6rem] uppercase tracking-[0.35em] sm:block" :class="light ? 'text-gold-200' : 'text-gold-600'">
        Fine Jewellery · Since 1985
      </span>
    </span>
  </NuxtLink>
</template>
