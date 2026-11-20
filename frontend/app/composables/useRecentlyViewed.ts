const MAX_ITEMS = 8

/** Remembers which ornaments the current visitor opened (per user, in localStorage). */
export function useRecentlyViewed() {
  const { user } = useAuth()
  const version = useState('recently-viewed-version', () => 0)

  const storageKey = computed(() => `recently-viewed:${user.value?.id ?? 'guest'}`)

  const ids = computed<string[]>(() => {
    void version.value // re-evaluate after track()/clear()
    return readStorage<string[]>(storageKey.value, [])
  })

  function track(id: string) {
    writeStorage(storageKey.value, [id, ...ids.value.filter(existing => existing !== id)].slice(0, MAX_ITEMS))
    version.value++
  }

  function clear() {
    removeStorage(storageKey.value)
    version.value++
  }

  return { ids, track, clear }
}
