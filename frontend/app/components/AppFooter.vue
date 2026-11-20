<script setup lang="ts">
import type { IconName } from '~/types'
import { categories } from '~/data/categories'

const year = new Date().getFullYear()
const mockMode = useRuntimeConfig().public.useMockApi

const quickLinks = [
  { label: 'Home', to: '/' },
  { label: 'Ornaments', to: '/ornaments' },
  { label: 'About Us', to: '/about' },
  { label: 'Contact', to: '/contact' },
  { label: 'My Account', to: '/dashboard' },
]

const socials: { label: string, href: string, icon: IconName }[] = [
  { label: 'Facebook', href: 'https://www.facebook.com/', icon: 'facebook' },
  { label: 'Instagram', href: 'https://www.instagram.com/', icon: 'instagram' },
  { label: 'YouTube', href: 'https://www.youtube.com/', icon: 'youtube' },
  { label: 'WhatsApp', href: 'https://wa.me/94771234567', icon: 'chat' },
]
</script>

<template>
  <footer class="mt-auto bg-charcoal text-gold-100/80">
    <div class="container-page grid gap-10 py-14 sm:grid-cols-2 lg:grid-cols-4">
      <div class="space-y-4">
        <AppLogo light />
        <p class="text-sm leading-relaxed">
          Three generations of Sri Lankan goldsmiths crafting heirloom jewellery with ethically sourced Ceylon gemstones.
        </p>
        <div class="flex gap-2">
          <a
            v-for="social in socials"
            :key="social.label"
            :href="social.href"
            target="_blank"
            rel="noopener noreferrer"
            :aria-label="social.label"
            class="grid size-9 place-items-center rounded-full border border-gold-100/20 transition hover:-translate-y-0.5 hover:border-gold-400 hover:bg-gold-500 hover:text-white"
          >
            <AppIcon :name="social.icon" :size="16" />
          </a>
        </div>
      </div>

      <div>
        <h3 class="mb-4 font-serif text-lg text-white">Quick Links</h3>
        <ul class="space-y-2 text-sm">
          <li v-for="link in quickLinks" :key="link.to">
            <NuxtLink :to="link.to" class="transition hover:text-gold-300">{{ link.label }}</NuxtLink>
          </li>
        </ul>
      </div>

      <div>
        <h3 class="mb-4 font-serif text-lg text-white">Collections</h3>
        <ul class="space-y-2 text-sm">
          <li v-for="category in categories" :key="category.id">
            <NuxtLink :to="{ path: '/ornaments', query: { category: category.id } }" class="transition hover:text-gold-300">
              {{ category.name }}
            </NuxtLink>
          </li>
        </ul>
      </div>

      <div>
        <h3 class="mb-4 font-serif text-lg text-white">Visit Us</h3>
        <ul class="space-y-3 text-sm">
          <li class="flex gap-3"><AppIcon name="map-pin" :size="18" class="mt-0.5 text-gold-400" /> 125 Main Street, Colombo 11, Sri Lanka</li>
          <li class="flex gap-3"><AppIcon name="phone" :size="18" class="text-gold-400" /> <a href="tel:+94112345678" class="hover:text-gold-300">+94 11 234 5678</a></li>
          <li class="flex gap-3"><AppIcon name="mail" :size="18" class="text-gold-400" /> <a href="mailto:hello@lankaornaments.lk" class="hover:text-gold-300">hello@lankaornaments.lk</a></li>
          <li class="flex gap-3"><AppIcon name="clock" :size="18" class="text-gold-400" /> Mon – Sat, 9.30 am – 7.00 pm</li>
        </ul>
      </div>
    </div>

    <div class="border-t border-gold-100/10">
      <div class="container-page flex flex-col items-center justify-between gap-2 py-5 text-xs sm:flex-row">
        <p>© {{ year }} Lanka Ornaments. All rights reserved.</p>
        <p v-if="mockMode" class="text-gold-100/50">Demo storefront · offline sample data</p>
      </div>
    </div>
  </footer>
</template>
