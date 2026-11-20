<script setup lang="ts">
import type { IconName } from '~/types'
import { categories } from '~/data/categories'

useHead({ title: 'Fine Sri Lankan Jewellery' })

const { items, featured, latest, loading, error, countByCategory, load } = useOrnaments()
const toast = useToast()
load()

const heroPieces = computed(() => featured.value.slice(0, 3))

const categoryCards = computed(() =>
  categories.map(category => ({
    ...category,
    count: countByCategory.value[category.id],
    sample: items.value.find(o => o.categoryId === category.id && o.gemstone) ?? items.value.find(o => o.categoryId === category.id),
  })),
)

const promises: { icon: IconName, title: string, text: string }[] = [
  { icon: 'shield', title: 'Certified Gemstones', text: 'Every stone is NGJA certified' },
  { icon: 'sparkles', title: 'Hallmarked Gold', text: '22K & 18K purity guaranteed' },
  { icon: 'heart', title: 'Lifetime Care', text: 'Free cleaning & polishing' },
  { icon: 'gift', title: 'Insured Delivery', text: 'Island-wide, gift wrapped' },
]

const newsletterEmail = ref('')
const newsletterError = ref<string>()

function subscribe() {
  const result = rules.email()(newsletterEmail.value)
  newsletterError.value = result === true ? undefined : result
  if (result !== true) return
  toast.success('Thank you! You will hear about our new collections first.')
  newsletterEmail.value = ''
}
</script>

<template>
  <div>
    <!-- Hero -->
    <section class="relative overflow-hidden bg-linear-to-br from-ivory via-cream to-beige">
      <div class="pointer-events-none absolute -right-32 -top-32 size-[32rem] rounded-full bg-gold-200/40 blur-3xl" />
      <div class="container-page relative grid items-center gap-12 py-16 lg:grid-cols-2 lg:py-24">
        <div class="animate-fade-up">
          <p class="eyebrow">Handcrafted in Sri Lanka</p>
          <h1 class="mt-4 text-[2.6rem] font-semibold leading-[1.05] text-charcoal sm:text-6xl lg:text-7xl">
            Timeless gold,<br>
            <span class="bg-linear-to-r from-gold-500 to-gold-700 bg-clip-text italic text-transparent">Ceylon’s finest gems</span>
          </h1>
          <p class="mt-6 max-w-lg text-lg leading-relaxed text-charcoal-soft">
            Discover heirloom rings, necklaces and bridal sets crafted by master goldsmiths with ethically sourced Sri Lankan sapphires.
          </p>
          <div class="mt-8 flex flex-wrap gap-3">
            <NuxtLink to="/ornaments" class="btn btn-primary px-8 py-3">
              Shop the collection <AppIcon name="arrow-right" :size="16" />
            </NuxtLink>
            <a href="#categories" class="btn btn-outline px-8 py-3">Explore categories</a>
          </div>
          <dl class="mt-12 grid max-w-md grid-cols-3 gap-6 border-t border-gold-200 pt-8">
            <div><dt class="text-xs uppercase tracking-wider text-charcoal-soft">Heritage</dt><dd class="font-serif text-3xl font-semibold">40+ yrs</dd></div>
            <div><dt class="text-xs uppercase tracking-wider text-charcoal-soft">Customers</dt><dd class="font-serif text-3xl font-semibold">12k+</dd></div>
            <div><dt class="text-xs uppercase tracking-wider text-charcoal-soft">Showrooms</dt><dd class="font-serif text-3xl font-semibold">3</dd></div>
          </dl>
        </div>

        <div class="relative mx-auto grid w-full max-w-lg grid-cols-2 gap-4">
          <template v-if="heroPieces.length === 3">
            <NuxtLink :to="`/ornaments/${heroPieces[0]!.id}`" class="group row-span-2 overflow-hidden rounded-[2rem] shadow-lift">
              <OrnamentImage :ornament="heroPieces[0]!" class="h-full min-h-80 transition-transform duration-700 group-hover:scale-105" />
            </NuxtLink>
            <NuxtLink v-for="piece in heroPieces.slice(1)" :key="piece.id" :to="`/ornaments/${piece.id}`" class="group overflow-hidden rounded-[2rem] shadow-soft">
              <OrnamentImage :ornament="piece" class="aspect-square transition-transform duration-700 group-hover:scale-105" />
            </NuxtLink>
          </template>
          <div v-else class="col-span-2 aspect-[4/3] animate-pulse rounded-[2rem] bg-gold-100" />
          <div class="card absolute -bottom-6 left-6 flex items-center gap-3 px-4 py-3 sm:left-auto sm:right-6">
            <span class="grid size-10 place-items-center rounded-full bg-gold-100 text-gold-700"><AppIcon name="sparkles" /></span>
            <div>
              <p class="text-xs text-charcoal-soft">New this season</p>
              <p class="font-serif text-lg font-semibold">Lotus Collection</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Brand promises -->
    <section class="border-y border-gold-100 bg-white">
      <div class="container-page grid grid-cols-2 gap-6 py-8 lg:grid-cols-4">
        <div v-for="promise in promises" :key="promise.title" class="flex items-center gap-3">
          <span class="grid size-11 shrink-0 place-items-center rounded-full bg-gold-50 text-gold-600"><AppIcon :name="promise.icon" /></span>
          <div>
            <p class="text-sm font-semibold">{{ promise.title }}</p>
            <p class="text-xs text-charcoal-soft">{{ promise.text }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured -->
    <section class="container-page py-20">
      <div class="mb-10 flex flex-wrap items-end justify-between gap-4">
        <div>
          <p class="eyebrow">Curated for you</p>
          <h2 class="section-title mt-2">Featured Ornaments</h2>
        </div>
        <NuxtLink to="/ornaments" class="btn btn-ghost">View all <AppIcon name="arrow-right" :size="16" /></NuxtLink>
      </div>
      <LoadingIndicator v-if="loading && !featured.length" label="Loading featured ornaments…" />
      <EmptyState v-else-if="error && !items.length" icon="warning" title="Our collection is temporarily unavailable" :description="error">
        <button type="button" class="btn btn-primary" @click="load(true)">Try again</button>
      </EmptyState>
      <div v-else class="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard v-for="ornament in featured.slice(0, 4)" :key="ornament.id" :ornament="ornament" />
      </div>
    </section>

    <!-- Categories -->
    <section id="categories" class="scroll-mt-24 bg-cream py-20">
      <div class="container-page">
        <div class="mx-auto mb-12 max-w-2xl text-center">
          <p class="eyebrow">Shop by category</p>
          <h2 class="section-title mt-2">Popular Categories</h2>
          <p class="mt-3 text-charcoal-soft">From everyday elegance to once-in-a-lifetime bridal pieces.</p>
        </div>
        <div class="grid grid-cols-2 gap-4 sm:gap-6 md:grid-cols-3 lg:grid-cols-5">
          <NuxtLink
            v-for="category in categoryCards"
            :key="category.id"
            :to="{ path: '/ornaments', query: { category: category.id } }"
            class="group card overflow-hidden text-center transition-all duration-300 hover:-translate-y-1 hover:shadow-lift"
          >
            <div class="overflow-hidden">
              <OrnamentImage
                v-if="category.sample"
                :ornament="category.sample"
                class="aspect-square transition-transform duration-700 group-hover:scale-110"
              />
              <div v-else class="aspect-square bg-gold-50" />
            </div>
            <div class="p-4">
              <h3 class="font-serif text-xl font-semibold">{{ category.name }}</h3>
              <p class="text-xs text-charcoal-soft">{{ category.count }} pieces</p>
            </div>
          </NuxtLink>
        </div>
      </div>
    </section>

    <!-- Promotion -->
    <section class="container-page py-20">
      <div class="relative overflow-hidden rounded-[2rem] bg-charcoal px-6 py-12 text-white sm:px-12 lg:py-16">
        <div class="absolute inset-0 bg-[radial-gradient(circle_at_85%_30%,rgb(195_154_82/0.45),transparent_50%)]" />
        <div class="relative grid items-center gap-10 lg:grid-cols-[1.3fr_1fr]">
          <div>
            <span class="badge bg-gold-500/20 text-gold-200 ring-1 ring-gold-400/40">Limited time · Until 31 December</span>
            <h2 class="mt-4 font-serif text-4xl font-semibold leading-tight sm:text-5xl">Festive Season Sale<br><span class="text-gold-300">15% off making charges</span></h2>
            <p class="mt-4 max-w-lg text-gold-100/80">
              Celebrate with something that lasts forever. Enjoy special savings on our necklace and bracelet collections, with complimentary gift packaging.
            </p>
            <div class="mt-8 flex flex-wrap gap-3">
              <NuxtLink :to="{ path: '/ornaments', query: { category: 'necklaces' } }" class="btn btn-primary px-8 py-3">Shop necklaces</NuxtLink>
              <NuxtLink :to="{ path: '/ornaments', query: { category: 'bracelets' } }" class="btn border border-gold-300/50 text-gold-100 hover:bg-white/10">Shop bracelets</NuxtLink>
            </div>
          </div>
          <div class="hidden grid-cols-2 gap-4 lg:grid">
            <OrnamentImage
              v-for="(piece, index) in items.filter(o => o.categoryId === 'necklaces' || o.categoryId === 'bracelets').slice(0, 2)"
              :key="piece.id"
              :ornament="piece"
              class="aspect-[3/4] rounded-2xl ring-1 ring-white/10"
              :class="index === 1 ? 'translate-y-8' : ''"
            />
          </div>
        </div>
      </div>
    </section>

    <!-- Latest -->
    <section class="container-page pb-20">
      <div class="mb-10 flex flex-wrap items-end justify-between gap-4">
        <div>
          <p class="eyebrow">Just arrived</p>
          <h2 class="section-title mt-2">Latest Products</h2>
        </div>
        <NuxtLink :to="{ path: '/ornaments', query: { sort: 'newest' } }" class="btn btn-ghost">See what’s new <AppIcon name="arrow-right" :size="16" /></NuxtLink>
      </div>
      <LoadingIndicator v-if="loading && !latest.length" label="Loading latest products…" />
      <div v-else class="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard v-for="ornament in latest.slice(0, 4)" :key="ornament.id" :ornament="ornament" />
      </div>
    </section>

    <!-- Newsletter -->
    <section class="bg-linear-to-r from-gold-100 via-cream to-gold-100">
      <div class="container-page flex flex-col items-center gap-6 py-14 text-center">
        <h2 class="section-title">Be the first to see new collections</h2>
        <p class="max-w-xl text-charcoal-soft">Join our list for private previews, festive offers and care tips from our goldsmiths.</p>
        <form class="flex w-full max-w-md flex-col gap-3 sm:flex-row" novalidate @submit.prevent="subscribe">
          <div class="flex-1 text-left">
            <label for="newsletter-email" class="sr-only">Email address</label>
            <input id="newsletter-email" v-model="newsletterEmail" type="email" placeholder="your@email.com" class="input rounded-full!" :class="{ 'input-error': newsletterError }">
            <p v-if="newsletterError" class="mt-1 pl-4 text-xs text-rose-600">{{ newsletterError }}</p>
          </div>
          <button type="submit" class="btn btn-primary self-start px-8 py-3 sm:self-auto">Subscribe</button>
        </form>
      </div>
    </section>
  </div>
</template>
