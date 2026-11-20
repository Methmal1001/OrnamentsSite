<script setup lang="ts">
import type { IconName } from '~/types'
import { seedOrnaments } from '~/data/ornaments'

useHead({ title: 'About Us' })

const values: { icon: IconName, title: string, text: string }[] = [
  { icon: 'shield', title: 'Integrity', text: 'Every gemstone is certified, and every gold piece is hallmarked for purity.' },
  { icon: 'sparkles', title: 'Craftsmanship', text: 'Our goldsmiths train for years in traditional Kandyan techniques.' },
  { icon: 'heart', title: 'Lifelong care', text: 'We clean, polish and repair our jewellery for as long as you own it.' },
]

const milestones = [
  { year: '1985', text: 'A small goldsmith workshop opens on Sea Street, Colombo.' },
  { year: '1998', text: 'We begin sourcing sapphires directly from Ratnapura mines.' },
  { year: '2012', text: 'Our flagship showroom opens on Main Street, Colombo 11.' },
  { year: '2026', text: 'Lanka Ornaments goes online to serve customers island-wide.' },
]

const showcase = seedOrnaments.filter(o => o.gemstone).slice(2, 4)
</script>

<template>
  <div>
    <section class="bg-linear-to-br from-cream to-beige">
      <div class="container-page grid items-center gap-12 py-16 lg:grid-cols-2 lg:py-24">
        <div>
          <p class="eyebrow">Our story</p>
          <h1 class="section-title mt-2 sm:text-5xl">Four decades of Sri Lankan craftsmanship</h1>
          <p class="mt-6 text-lg leading-relaxed text-charcoal-soft">
            Lanka Ornaments began as a family workshop with a simple promise: honest gold, genuine Ceylon gems and jewellery made to be passed down.
            Today, three generations later, that promise still guides every piece we create.
          </p>
          <NuxtLink to="/contact" class="btn btn-primary mt-8 px-8 py-3">Visit our showroom</NuxtLink>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <OrnamentImage v-for="(item, index) in showcase" :key="item.id" :ornament="item" class="aspect-[3/4] rounded-[2rem] shadow-lift" :class="index === 1 ? 'mt-10' : ''" />
        </div>
      </div>
    </section>

    <section class="container-page py-20">
      <div class="grid gap-6 md:grid-cols-3">
        <div v-for="value in values" :key="value.title" class="card p-8 transition duration-300 hover:-translate-y-1 hover:shadow-lift">
          <span class="grid size-12 place-items-center rounded-2xl bg-gold-50 text-gold-600"><AppIcon :name="value.icon" :size="24" /></span>
          <h2 class="mt-5 text-2xl font-semibold">{{ value.title }}</h2>
          <p class="mt-2 text-charcoal-soft">{{ value.text }}</p>
        </div>
      </div>
    </section>

    <section class="bg-cream py-20">
      <div class="container-page max-w-3xl">
        <h2 class="section-title mb-10 text-center">Our journey</h2>
        <ol class="relative space-y-8 border-l border-gold-300 pl-8">
          <li v-for="milestone in milestones" :key="milestone.year" class="relative">
            <span class="absolute -left-[2.45rem] top-1 size-4 rounded-full border-4 border-cream bg-gold-500" />
            <p class="font-serif text-2xl font-semibold text-gold-700">{{ milestone.year }}</p>
            <p class="mt-1 text-charcoal-soft">{{ milestone.text }}</p>
          </li>
        </ol>
      </div>
    </section>
  </div>
</template>
