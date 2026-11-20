<script setup lang="ts">
import type { Ornament } from '~/types'

defineProps<{ ornament: Ornament }>()
</script>

<template>
  <article class="card group flex h-full flex-col overflow-hidden transition-all duration-300 hover:-translate-y-1 hover:shadow-lift">
    <NuxtLink :to="`/ornaments/${ornament.id}`" class="relative block overflow-hidden" :aria-label="`View ${ornament.name}`">
      <OrnamentImage :ornament="ornament" class="aspect-square transition-transform duration-700 group-hover:scale-105" />
      <span class="badge absolute left-3 top-3" :class="availabilityMeta[ornament.availability].classes">
        {{ availabilityMeta[ornament.availability].label }}
      </span>
      <span v-if="ornament.isFeatured" class="badge absolute right-3 top-3 bg-charcoal/85 text-gold-200">
        <AppIcon name="sparkles" :size="12" /> Featured
      </span>
    </NuxtLink>

    <div class="flex flex-1 flex-col p-5">
      <p class="eyebrow text-[0.65rem]">{{ categoryName(ornament.categoryId) }}</p>
      <h3 class="mt-1.5 font-serif text-xl font-semibold leading-snug text-charcoal">
        <NuxtLink :to="`/ornaments/${ornament.id}`" class="transition-colors hover:text-gold-700">{{ ornament.name }}</NuxtLink>
      </h3>
      <p class="mt-2 line-clamp-2 text-sm leading-relaxed text-charcoal-soft">{{ ornament.description }}</p>

      <div class="mt-auto flex items-end justify-between gap-3 pt-5">
        <div>
          <p class="text-xs text-charcoal-soft">{{ ornament.metal }}</p>
          <p class="text-lg font-semibold text-gold-700">{{ formatPrice(ornament.price) }}</p>
        </div>
        <NuxtLink :to="`/ornaments/${ornament.id}`" class="btn btn-outline btn-sm group/btn">
          View details
          <AppIcon name="arrow-right" :size="14" class="transition-transform group-hover/btn:translate-x-0.5" />
        </NuxtLink>
      </div>
    </div>
  </article>
</template>
