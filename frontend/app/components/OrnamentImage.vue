<script setup lang="ts">
import type { Ornament } from '~/types'

/**
 * Shows the product photo when `imageUrl` is set; otherwise renders an
 * elegant SVG illustration based on the category, metal and gemstone.
 * Size it from the parent (e.g. `class="aspect-square"`).
 */
const props = defineProps<{
  ornament: Pick<Ornament, 'name' | 'categoryId' | 'gemstone' | 'metal' | 'imageUrl'>
}>()

const uid = useId()
const imageFailed = ref(false)
watch(() => props.ornament.imageUrl, () => { imageFailed.value = false })

const showPhoto = computed(() => !!props.ornament.imageUrl && !imageFailed.value)

type Palette = [light: string, mid: string, dark: string]

const metalPalette = computed<Palette>(() => {
  const metal = props.ornament.metal.toLowerCase()
  if (metal.includes('white')) return ['#fbfbfa', '#d3d6d9', '#8f959b']
  if (metal.includes('rose')) return ['#fbe1d3', '#d99a7e', '#9c5b44']
  return ['#fdf0c6', '#d6ad5e', '#8e682e']
})

const gemPalettes: Record<string, Palette> = {
  'blue sapphire': ['#9db4f0', '#2f4ea3', '#14214f'],
  'ruby': ['#f598a8', '#b0213f', '#5a0c1d'],
  'emerald': ['#9be5c0', '#1f7a55', '#0a3a28'],
  'pearl': ['#ffffff', '#f1ebe0', '#c9bfae'],
  'diamond': ['#ffffff', '#e4edf4', '#9fb4c5'],
  'moonstone': ['#f9fcff', '#d5e1f0', '#97abc7'],
  'padparadscha': ['#ffd8c7', '#f28c6e', '#ad4c36'],
  'yellow sapphire': ['#fff3b8', '#e7b93a', '#a07212'],
  'cat\'s eye': ['#f6eab0', '#b9a246', '#675718'],
}

const hasGem = computed(() => !!props.ornament.gemstone)
const gemPalette = computed<Palette>(() =>
  gemPalettes[(props.ornament.gemstone ?? '').toLowerCase()] ?? metalPalette.value,
)
const isCatsEye = computed(() => (props.ornament.gemstone ?? '').toLowerCase() === 'cat\'s eye')

const backgroundTint: Record<Ornament['categoryId'], string> = {
  'gold-rings': '#f6ecd9',
  'necklaces': '#f2e7df',
  'bracelets': '#ece9df',
  'earrings': '#f4e6df',
  'pendants': '#efe9db',
}

/** Teardrop gem outline: point at (cx, top), bulb of radius r below it. */
function drop(cx: number, top: number, r: number) {
  const bulbY = top + r * 1.8
  return `M ${cx} ${top} C ${cx + r * 0.55} ${top + r * 0.9}, ${cx + r} ${top + r * 1.25}, ${cx + r} ${bulbY} `
    + `A ${r} ${r} 0 0 1 ${cx - r} ${bulbY} C ${cx - r} ${top + r * 1.25}, ${cx - r * 0.55} ${top + r * 0.9}, ${cx} ${top} Z`
}

function sparkle(x: number, y: number, s: number) {
  const t = s / 4
  return `M ${x} ${y - s} L ${x + t} ${y - t} L ${x + s} ${y} L ${x + t} ${y + t} L ${x} ${y + s} L ${x - t} ${y + t} L ${x - s} ${y} L ${x - t} ${y - t} Z`
}

const sparkles = [sparkle(92, 96, 9), sparkle(318, 128, 6), sparkle(300, 300, 7)]

const braceletGems = [30, 60, 90, 120, 150].map((deg) => {
  const rad = (deg * Math.PI) / 180
  return { x: 200 + 120 * Math.cos(rad), y: 215 + 60 * Math.sin(rad) }
})

const id = (name: string) => `${uid}-${name}`
const url = (name: string) => `url(#${id(name)})`
</script>

<template>
  <div class="relative overflow-hidden" :style="{ backgroundColor: backgroundTint[ornament.categoryId] }">
    <img
      v-if="showPhoto"
      :src="ornament.imageUrl ?? undefined"
      :alt="ornament.name"
      class="absolute inset-0 h-full w-full object-cover"
      loading="lazy"
      @error="imageFailed = true"
    >
    <svg
      v-else
      class="absolute inset-0 h-full w-full"
      viewBox="0 0 400 400"
      preserveAspectRatio="xMidYMid meet"
      role="img"
      :aria-label="ornament.name"
    >
      <defs>
        <radialGradient :id="id('bg')" gradientUnits="userSpaceOnUse" cx="200" cy="180" r="330">
          <stop offset="0%" stop-color="#fffdf8" />
          <stop offset="100%" :stop-color="backgroundTint[ornament.categoryId]" />
        </radialGradient>
        <linearGradient :id="id('metal')" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" :stop-color="metalPalette[0]" />
          <stop offset="45%" :stop-color="metalPalette[1]" />
          <stop offset="70%" :stop-color="metalPalette[0]" />
          <stop offset="100%" :stop-color="metalPalette[2]" />
        </linearGradient>
        <radialGradient :id="id('gem')" cx="40%" cy="35%" r="75%" fx="35%" fy="28%">
          <stop offset="0%" :stop-color="gemPalette[0]" />
          <stop offset="50%" :stop-color="gemPalette[1]" />
          <stop offset="100%" :stop-color="gemPalette[2]" />
        </radialGradient>
        <radialGradient :id="id('shadow')">
          <stop offset="0%" stop-color="#5b4329" stop-opacity="0.18" />
          <stop offset="100%" stop-color="#5b4329" stop-opacity="0" />
        </radialGradient>
      </defs>

      <rect x="-400" y="-400" width="1200" height="1200" :fill="url('bg')" />
      <ellipse cx="200" cy="335" rx="125" ry="16" :fill="url('shadow')" />

      <!-- Gold rings -->
      <g v-if="ornament.categoryId === 'gold-rings'">
        <ellipse cx="200" cy="250" rx="94" ry="36" fill="none" :stroke="url('metal')" stroke-width="18" />
        <ellipse cx="200" cy="246" rx="94" ry="36" fill="none" stroke="#fff" stroke-opacity="0.45" stroke-width="2" />
        <template v-if="hasGem">
          <path d="M184 214 L178 196 M216 214 L222 196 M200 216 L200 200" :stroke="url('metal')" stroke-width="6" stroke-linecap="round" />
          <polygon points="176,160 224,160 248,184 200,232 152,184" :fill="url('gem')" />
          <path d="M152 184 H248 M176 160 L188 184 L200 232 L212 184 L224 160 M188 184 L200 160 L212 184" fill="none" stroke="#fff" stroke-opacity="0.45" stroke-width="1.5" />
        </template>
        <ellipse v-else cx="200" cy="250" rx="94" ry="36" fill="none" :stroke="metalPalette[2]" stroke-opacity="0.35" stroke-width="1.5" stroke-dasharray="4 6" />
      </g>

      <!-- Necklaces -->
      <g v-else-if="ornament.categoryId === 'necklaces'">
        <path d="M90 80 C 100 250 300 250 310 80" fill="none" :stroke="metalPalette[1]" stroke-width="2" />
        <path d="M90 80 C 100 250 300 250 310 80" fill="none" :stroke="url('metal')" stroke-width="8" stroke-linecap="round" stroke-dasharray="0.1 12" />
        <circle cx="140" cy="182" r="9" :fill="url('gem')" :stroke="url('metal')" stroke-width="3" />
        <circle cx="260" cy="182" r="9" :fill="url('gem')" :stroke="url('metal')" stroke-width="3" />
        <circle cx="200" cy="203" r="16" :fill="url('metal')" />
        <path :d="drop(200, 214, 26)" :fill="url('metal')" />
        <path :d="drop(200, 222, 20)" :fill="url('gem')" />
        <ellipse cx="192" cy="252" rx="5" ry="8" fill="#fff" fill-opacity="0.5" />
      </g>

      <!-- Bracelets -->
      <g v-else-if="ornament.categoryId === 'bracelets'">
        <ellipse cx="200" cy="215" rx="120" ry="60" fill="none" :stroke="url('metal')" stroke-width="20" />
        <ellipse cx="200" cy="210" rx="120" ry="60" fill="none" stroke="#fff" stroke-opacity="0.4" stroke-width="2" />
        <template v-if="hasGem">
          <circle v-for="(gem, index) in braceletGems" :key="index" :cx="gem.x" :cy="gem.y" r="8" :fill="url('gem')" stroke="#fff" stroke-opacity="0.6" stroke-width="1.5" />
        </template>
        <ellipse v-else cx="200" cy="215" rx="120" ry="60" fill="none" :stroke="metalPalette[2]" stroke-opacity="0.45" stroke-width="16" stroke-dasharray="3 7" />
      </g>

      <!-- Earrings -->
      <g v-else-if="ornament.categoryId === 'earrings'">
        <g v-for="x in [140, 260]" :key="x">
          <path :d="`M ${x} 128 L ${x} 108 Q ${x} 82 ${x + 20} 82 Q ${x + 38} 82 ${x + 38} 104`" fill="none" :stroke="url('metal')" stroke-width="4" stroke-linecap="round" />
          <circle :cx="x" :cy="134" r="9" :fill="url('metal')" />
          <path :d="`M ${x} 143 L ${x} 162`" :stroke="url('metal')" stroke-width="3" />
          <path :d="drop(x, 158, 30)" :fill="url('metal')" />
          <path :d="drop(x, 166, 24)" :fill="url('gem')" />
          <ellipse :cx="x - 8" :cy="202" rx="5" ry="9" fill="#fff" fill-opacity="0.5" />
        </g>
      </g>

      <!-- Pendants -->
      <g v-else>
        <path d="M100 50 Q 200 240 300 50" fill="none" :stroke="url('metal')" stroke-width="3" />
        <rect x="191" y="132" width="18" height="26" rx="8" :fill="url('metal')" />
        <path :d="drop(200, 152, 48)" :fill="url('metal')" />
        <path :d="drop(200, 162, 40)" :fill="url('gem')" />
        <ellipse v-if="isCatsEye" cx="200" cy="236" rx="4" ry="34" fill="#fffbe6" fill-opacity="0.75" />
        <ellipse v-else cx="186" cy="226" rx="8" ry="14" fill="#fff" fill-opacity="0.45" />
      </g>

      <path v-for="(d, index) in sparkles" :key="index" :d="d" fill="#fff" fill-opacity="0.9" :stroke="metalPalette[1]" stroke-opacity="0.5" stroke-width="0.8" />
    </svg>
  </div>
</template>
