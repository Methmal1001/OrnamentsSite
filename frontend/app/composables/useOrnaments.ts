import type { AvailabilityStatus, CategoryId, Ornament, OrnamentInput, OrnamentSort } from '~/types'
import { categories } from '~/data/categories'

/** Full catalogue shared by the home page, dashboards and the admin console. */
export function useOrnaments() {
  const { ornaments: service } = useServices()

  const items = useState<Ornament[]>('ornaments', () => [])
  const loaded = useState('ornaments-loaded', () => false)
  const loading = useState('ornaments-loading', () => false)
  const error = useState<string | null>('ornaments-error', () => null)

  async function load(force = false) {
    if ((loaded.value && !force) || loading.value) return
    loading.value = true
    error.value = null
    try {
      items.value = await service.list()
      loaded.value = true
    }
    catch (e) {
      error.value = errorMessage(e, 'Could not load ornaments')
    }
    finally {
      loading.value = false
    }
  }

  /** Uses the cached catalogue when possible, otherwise asks the API (null = not found). */
  async function fetchById(id: string): Promise<Ornament | null> {
    const cached = items.value.find(o => o.id === id)
    if (cached) return cached
    return service.get(id)
  }

  async function create(input: OrnamentInput) {
    const created = await service.create(input)
    items.value = [created, ...items.value]
    return created
  }

  async function update(id: string, input: OrnamentInput) {
    const updated = await service.update(id, input)
    items.value = items.value.map(o => (o.id === id ? updated : o))
    return updated
  }

  async function remove(id: string) {
    await service.remove(id)
    items.value = items.value.filter(o => o.id !== id)
  }

  const featured = computed(() => items.value.filter(o => o.isFeatured))
  const latest = computed(() => [...items.value].sort((a, b) => b.createdAt.localeCompare(a.createdAt)))
  const countByCategory = computed(() =>
    Object.fromEntries(categories.map(c => [c.id, items.value.filter(o => o.categoryId === c.id).length])) as Record<CategoryId, number>,
  )

  return {
    items, loading, loaded, error, featured, latest, countByCategory,
    load, fetchById, create, update, remove,
  }
}

/**
 * Search, category, availability and sort for the shop page. Results come from the
 * API (GET /api/ornaments?search=…), debounced while typing; stale responses are ignored.
 */
export function useOrnamentSearch() {
  const { ornaments: service } = useServices()

  const search = ref('')
  const category = ref<CategoryId | 'all'>('all')
  const availability = ref<AvailabilityStatus | 'all'>('all')
  const sort = ref<OrnamentSort>('featured')

  const results = ref<Ornament[]>([])
  const loading = ref(false)
  const loaded = ref(false)
  const error = ref<string | null>(null)

  let requestId = 0
  let debounce: ReturnType<typeof setTimeout> | undefined

  async function fetchResults() {
    clearTimeout(debounce)
    const id = ++requestId
    loading.value = true
    error.value = null
    try {
      const list = await service.list({
        search: search.value.trim() || undefined,
        category: category.value === 'all' ? undefined : category.value,
        availability: availability.value === 'all' ? undefined : availability.value,
        sort: sort.value,
      })
      if (id === requestId) {
        results.value = list
        loaded.value = true
      }
    }
    catch (e) {
      if (id === requestId) error.value = errorMessage(e, 'Could not load ornaments')
    }
    finally {
      if (id === requestId) loading.value = false
    }
  }

  watch([category, availability, sort], () => fetchResults())
  watch(search, () => {
    clearTimeout(debounce)
    debounce = setTimeout(fetchResults, 300)
  })
  onBeforeUnmount(() => clearTimeout(debounce))

  const hasActiveFilters = computed(() =>
    search.value.trim() !== '' || category.value !== 'all' || availability.value !== 'all')

  function reset() {
    search.value = ''
    category.value = 'all'
    availability.value = 'all'
    sort.value = 'featured'
  }

  return { search, category, availability, sort, results, loading, loaded, error, hasActiveFilters, fetchResults, reset }
}

/** Client-side search/category filter for lists already in memory (admin console). */
export function useOrnamentFilters(source: Ref<Ornament[]>) {
  const search = ref('')
  const category = ref<CategoryId | 'all'>('all')
  const sort = ref<OrnamentSort>('featured')

  const filtered = computed(() => filterOrnaments(source.value, {
    search: search.value,
    category: category.value === 'all' ? undefined : category.value,
    sort: sort.value,
  }))

  const hasActiveFilters = computed(() => search.value.trim() !== '' || category.value !== 'all')

  function reset() {
    search.value = ''
    category.value = 'all'
    sort.value = 'featured'
  }

  return { search, category, sort, filtered, hasActiveFilters, reset }
}
