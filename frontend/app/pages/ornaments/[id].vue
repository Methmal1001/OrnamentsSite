<script setup lang="ts">
import type { Ornament } from '~/types'

const route = useRoute()
const { items, load, fetchById } = useOrnaments()
const { track } = useRecentlyViewed()
const { isAuthenticated } = useAuth()
const toast = useToast()

const ornament = ref<Ornament | null>(null)
const loading = ref(true)
const loadError = ref<string | null>(null)

async function loadOrnament(id: string) {
  loading.value = true
  loadError.value = null
  try {
    load() // warm the catalogue for "You may also like" without blocking this page
    ornament.value = await fetchById(id)
    if (ornament.value) track(ornament.value.id)
  }
  catch (error) {
    ornament.value = null
    loadError.value = errorMessage(error, 'Could not load this ornament.')
  }
  finally {
    loading.value = false
  }
}

watch(() => route.params.id, id => loadOrnament(String(id)), { immediate: true })

useHead({ title: () => ornament.value?.name ?? 'Ornament' })

const related = computed(() =>
  ornament.value
    ? items.value.filter(o => o.categoryId === ornament.value!.categoryId && o.id !== ornament.value!.id).slice(0, 4)
    : [],
)

const specifications = computed(() => {
  const o = ornament.value
  if (!o) return []
  return [
    { label: 'SKU', value: o.sku },
    { label: 'Category', value: categoryName(o.categoryId) },
    { label: 'Metal', value: o.metal },
    { label: 'Gemstone', value: o.gemstone ?? 'None' },
    { label: 'Approx. weight', value: `${o.weightGrams} g` },
    { label: 'Availability', value: availabilityMeta[o.availability].label },
  ]
})

const canEnquire = computed(() => ornament.value?.availability !== 'out-of-stock')

function enquire() {
  if (!ornament.value) return
  toast.success(`Thank you! Our jewellery consultant will contact you about “${ornament.value.name}”.`)
}

function reserve() {
  if (!isAuthenticated.value) {
    toast.info('Please log in to reserve a viewing appointment.')
    return navigateTo({ path: '/login', query: { redirect: route.fullPath } })
  }
  toast.success('Showroom viewing requested. Online reservations go live with the backend.')
}
</script>

<template>
  <div class="container-page py-10">
    <nav class="mb-8 flex flex-wrap items-center gap-1.5 text-sm text-charcoal-soft" aria-label="Breadcrumb">
      <NuxtLink to="/" class="hover:text-gold-700">Home</NuxtLink>
      <AppIcon name="chevron-right" :size="14" />
      <NuxtLink to="/ornaments" class="hover:text-gold-700">Ornaments</NuxtLink>
      <template v-if="ornament">
        <AppIcon name="chevron-right" :size="14" />
        <NuxtLink :to="{ path: '/ornaments', query: { category: ornament.categoryId } }" class="hover:text-gold-700">{{ categoryName(ornament.categoryId) }}</NuxtLink>
        <AppIcon name="chevron-right" :size="14" />
        <span class="truncate text-charcoal">{{ ornament.name }}</span>
      </template>
    </nav>

    <LoadingIndicator v-if="loading" label="Loading ornament…" />

    <EmptyState v-else-if="loadError" icon="warning" title="Unable to load this ornament" :description="loadError">
      <button type="button" class="btn btn-primary" @click="loadOrnament(String(route.params.id))">Try again</button>
      <NuxtLink to="/ornaments" class="btn btn-outline">Browse ornaments</NuxtLink>
    </EmptyState>

    <EmptyState v-else-if="!ornament" icon="search" title="Ornament not found" description="This piece may have been sold or removed from our collection.">
      <NuxtLink to="/ornaments" class="btn btn-primary">Browse ornaments</NuxtLink>
    </EmptyState>

    <template v-else>
      <div class="grid gap-10 lg:grid-cols-2 lg:gap-16">
        <div class="lg:sticky lg:top-32 lg:self-start">
          <div class="card overflow-hidden p-2">
            <OrnamentImage :ornament="ornament" class="aspect-square rounded-xl" />
          </div>
        </div>

        <div class="animate-fade-up">
          <div class="flex flex-wrap items-center gap-2">
            <span class="eyebrow">{{ categoryName(ornament.categoryId) }}</span>
            <span class="badge" :class="availabilityMeta[ornament.availability].classes">{{ availabilityMeta[ornament.availability].label }}</span>
            <span v-if="ornament.isFeatured" class="badge bg-charcoal text-gold-200"><AppIcon name="sparkles" :size="12" /> Featured</span>
          </div>
          <h1 class="mt-3 text-4xl font-semibold leading-tight sm:text-5xl">{{ ornament.name }}</h1>
          <p class="mt-4 font-serif text-3xl font-semibold text-gold-700">{{ formatPrice(ornament.price) }}</p>
          <p class="mt-1 text-xs text-charcoal-soft">Indicative price. Final price depends on the daily gold rate.</p>

          <p class="mt-6 text-lg leading-relaxed text-charcoal-soft">{{ ornament.description }}</p>

          <div class="mt-8 flex flex-wrap gap-3">
            <button type="button" class="btn btn-primary px-8 py-3" :disabled="!canEnquire" @click="reserve">
              <AppIcon name="clock" :size="16" /> Reserve a viewing
            </button>
            <button type="button" class="btn btn-outline px-8 py-3" @click="enquire">
              <AppIcon name="chat" :size="16" /> Enquire
            </button>
          </div>
          <p v-if="!canEnquire" class="mt-2 text-sm text-rose-600">This piece is currently out of stock. Send an enquiry to be notified when it returns.</p>

          <section class="card mt-10 overflow-hidden">
            <h2 class="border-b border-gold-100 bg-ivory px-6 py-4 font-serif text-xl font-semibold">Product information</h2>
            <dl class="divide-y divide-gold-100">
              <div v-for="spec in specifications" :key="spec.label" class="grid grid-cols-2 gap-4 px-6 py-3 text-sm">
                <dt class="text-charcoal-soft">{{ spec.label }}</dt>
                <dd class="font-medium text-charcoal">{{ spec.value }}</dd>
              </div>
            </dl>
          </section>

          <ul class="mt-6 grid gap-3 text-sm text-charcoal-soft sm:grid-cols-2">
            <li class="flex items-center gap-2"><AppIcon name="shield" :size="18" class="text-gold-600" /> Certified &amp; hallmarked</li>
            <li class="flex items-center gap-2"><AppIcon name="gift" :size="18" class="text-gold-600" /> Complimentary gift box</li>
            <li class="flex items-center gap-2"><AppIcon name="heart" :size="18" class="text-gold-600" /> Lifetime cleaning service</li>
            <li class="flex items-center gap-2"><AppIcon name="sparkles" :size="18" class="text-gold-600" /> Free resizing within 30 days</li>
          </ul>
        </div>
      </div>

      <section v-if="related.length" class="mt-20">
        <h2 class="section-title mb-8">You may also like</h2>
        <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <ProductCard v-for="item in related" :key="item.id" :ornament="item" />
        </div>
      </section>
    </template>
  </div>
</template>
