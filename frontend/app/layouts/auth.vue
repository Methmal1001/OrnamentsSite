<script setup lang="ts">
import { seedOrnaments } from '~/data/ornaments'

// Decorative showcase pieces for the brand panel.
const showcase = seedOrnaments.filter(o => o.isFeatured).slice(0, 3)
</script>

<template>
  <div class="grid min-h-screen lg:grid-cols-2">
    <section class="relative hidden overflow-hidden bg-charcoal lg:flex lg:flex-col lg:justify-between lg:p-12">
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_30%_20%,rgb(195_154_82/0.35),transparent_55%)]" />
      <AppLogo light class="relative" />

      <div class="relative grid grid-cols-3 gap-4">
        <OrnamentImage
          v-for="(item, index) in showcase"
          :key="item.id"
          :ornament="item"
          class="aspect-[3/4] rounded-2xl shadow-lift ring-1 ring-white/10"
          :class="index === 1 ? '-translate-y-6' : 'translate-y-4'"
        />
      </div>

      <blockquote class="relative max-w-md">
        <p class="font-serif text-3xl leading-snug text-white">“Every piece carries the warmth of Ceylon gold and the patience of our artisans.”</p>
        <footer class="mt-4 text-sm tracking-wide text-gold-200">— The Lanka Ornaments family</footer>
      </blockquote>
    </section>

    <section class="flex flex-col bg-linear-to-b from-ivory to-cream">
      <div class="flex items-center justify-between p-6 lg:justify-end">
        <AppLogo class="lg:hidden" />
        <NuxtLink to="/" class="btn btn-ghost btn-sm"><AppIcon name="arrow-left" :size="14" /> Back to store</NuxtLink>
      </div>
      <main class="flex flex-1 items-center justify-center px-4 pb-12 sm:px-8">
        <div class="w-full max-w-md animate-fade-up">
          <slot />
        </div>
      </main>
    </section>
  </div>
</template>
