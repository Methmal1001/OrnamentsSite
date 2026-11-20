import type { Ornament, OrnamentQuery, OrnamentSort } from '~/types'
import { categoryName } from './format'

const sorters: Record<OrnamentSort, (a: Ornament, b: Ornament) => number> = {
  'featured': (a, b) => Number(b.isFeatured) - Number(a.isFeatured) || b.createdAt.localeCompare(a.createdAt),
  'newest': (a, b) => b.createdAt.localeCompare(a.createdAt),
  'price-asc': (a, b) => a.price - b.price,
  'price-desc': (a, b) => b.price - a.price,
}

/**
 * Filters and sorts ornaments in the browser with the same rules as GET /api/ornaments.
 * Used by the mock service and by admin lists that already hold the full catalogue.
 */
export function filterOrnaments(list: Ornament[], query: OrnamentQuery = {}): Ornament[] {
  const term = query.search?.trim().toLowerCase() ?? ''
  return list
    .filter((o) => {
      if (query.category && o.categoryId !== query.category) return false
      if (query.availability && o.availability !== query.availability) return false
      if (!term) return true
      return [o.name, o.description, o.sku, o.metal, o.gemstone ?? '', categoryName(o.categoryId)]
        .some(field => field.toLowerCase().includes(term))
    })
    .sort(sorters[query.sort ?? 'featured'])
}
