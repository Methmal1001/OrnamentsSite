<script setup lang="ts">
import type { AvailabilityStatus, CategoryId, OrnamentSort } from '~/types'
import { categories } from '~/data/categories'

useHead({ title: 'Ornaments' })

const route = useRoute()
const router = useRouter()
const {
  search, category, availability, sort, results, loading, loaded, error, hasActiveFilters, fetchResults, reset,
} = useOrnamentSearch()

const sortOptions: { value: OrnamentSort, label: string }[] = [
  { value: 'featured', label: 'Featured' },
  { value: 'newest', label: 'Newest' },
  { value: 'price-asc', label: 'Price: Low to High' },
  { value: 'price-desc', label: 'Price: High to Low' },
]

const availabilityOptions: { value: AvailabilityStatus | 'all', label: string }[] = [
  { value: 'all', label: 'Any availability' },
  ...(Object.entries(availabilityMeta) as [AvailabilityStatus, { label: string }][]).map(([value, meta]) => ({ value, label: meta.label })),
]

const categoryOptions: { id: CategoryId | 'all', name: string }[] = [{ id: 'all', name: 'All' }, ...categories]

const isCategory = (value: unknown): value is CategoryId => categories.some(c => c.id === value)
const isSort = (value: unknown): value is OrnamentSort => sortOptions.some(o => o.value === value)
const isAvailability = (value: unknown): value is AvailabilityStatus => value !== 'all' && availabilityOptions.some(o => o.value === value)

// URL → filters (supports links such as /ornaments?category=necklaces)
watch(() => route.query, (query) => {
  category.value = isCategory(query.category) ? query.category : 'all'
  sort.value = isSort(query.sort) ? query.sort : 'featured'
  availability.value = isAvailability(query.availability) ? query.availability : 'all'
  const q = typeof query.q === 'string' ? query.q : ''
  if (q !== search.value.trim()) search.value = q
}, { immediate: true })

// Filters → URL (keeps the view shareable and the back button meaningful)
watch([category, sort, availability, search], ([c, s, a, q]) => {
  const query: Record<string, string> = {}
  if (c !== 'all') query.category = c
  if (s !== 'featured') query.sort = s
  if (a !== 'all') query.availability = a
  if (q.trim()) query.q = q.trim()
  const current = route.query
  if (current.category !== query.category || current.sort !== query.sort
    || current.availability !== query.availability || current.q !== query.q) {
    router.replace({ query })
  }
})

// First load (filters from the URL are already applied above).
fetchResults()

const activeCategory = computed(() => categories.find(c => c.id === category.value))
</script>

<template>
  <div>
    <section class="bg-linear-to-br from-cream to-beige">
      <div class="container-page py-14 text-center">
        <p class="eyebrow">The collection</p>
        <h1 class="section-title mt-2 sm:text-5xl">{{ activeCategory?.name ?? 'All Ornaments' }}</h1>
        <p class="mx-auto mt-3 max-w-xl text-charcoal-soft">
          {{ activeCategory?.description ?? 'Browse handcrafted gold jewellery set with Ceylon sapphires, rubies, pearls and more.' }}
        </p>
      </div>
    </section>

    <div class="container-page py-10">
      <!-- Filters -->
      <div class="card mb-8 space-y-4 p-4 sm:p-5">
        <div class="flex flex-col gap-3 lg:flex-row lg:items-center">
          <div class="flex-1">
            <SearchBar v-model="search" label="ornaments" placeholder="Search by name, gemstone or metal…" />
          </div>
          <div class="grid gap-3 sm:grid-cols-2 lg:flex lg:items-center">
            <div>
              <label for="availability" class="sr-only">Availability</label>
              <select id="availability" v-model="availability" class="input rounded-full! py-3 lg:w-48">
                <option v-for="option in availabilityOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>
            </div>
            <div class="flex items-center gap-2">
              <label for="sort" class="whitespace-nowrap text-sm text-charcoal-soft">Sort by</label>
              <select id="sort" v-model="sort" class="input rounded-full! py-3 lg:w-52">
                <option v-for="option in sortOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>
            </div>
          </div>
        </div>
        <div class="-mx-1 flex gap-2 overflow-x-auto px-1 pb-1" role="group" aria-label="Filter by category">
          <button
            v-for="option in categoryOptions"
            :key="option.id"
            type="button"
            class="cursor-pointer whitespace-nowrap rounded-full border px-4 py-2 text-sm font-medium transition-all duration-200"
            :class="category === option.id
              ? 'border-gold-500 bg-gold-500 text-white shadow-soft'
              : 'border-gold-200 bg-white text-charcoal-soft hover:border-gold-400 hover:text-gold-700'"
            :aria-pressed="category === option.id"
            @click="category = option.id"
          >
            {{ option.name }}
          </button>
        </div>
      </div>

      <div class="mb-6 flex items-center justify-between gap-4 text-sm text-charcoal-soft">
        <p class="flex items-center gap-2" aria-live="polite">
          <span v-if="loaded">
            <span class="font-semibold text-charcoal">{{ results.length }}</span> {{ results.length === 1 ? 'piece' : 'pieces' }} found
          </span>
          <LoadingIndicator v-if="loading && loaded" variant="inline" label="Updating results" />
        </p>
        <button v-if="hasActiveFilters" type="button" class="btn btn-ghost btn-sm" @click="reset">
          <AppIcon name="close" :size="14" /> Clear filters
        </button>
      </div>

      <LoadingIndicator v-if="loading && !loaded" label="Loading ornaments…" />
      <EmptyState v-else-if="error" icon="warning" title="Unable to load ornaments" :description="error">
        <button type="button" class="btn btn-primary" @click="fetchResults">Try again</button>
      </EmptyState>
      <EmptyState
        v-else-if="loaded && !results.length"
        icon="search"
        title="No ornaments match your search"
        description="Try a different keyword, availability or category."
      >
        <button type="button" class="btn btn-primary" @click="reset">Show all ornaments</button>
      </EmptyState>
      <TransitionGroup
        v-else
        tag="div"
        class="grid gap-6 transition-opacity duration-200 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        :class="{ 'opacity-60': loading }"
        enter-active-class="transition duration-300"
        enter-from-class="opacity-0 scale-95"
      >
        <ProductCard v-for="ornament in results" :key="ornament.id" :ornament="ornament" />
      </TransitionGroup>
    </div>
  </div>
</template>
