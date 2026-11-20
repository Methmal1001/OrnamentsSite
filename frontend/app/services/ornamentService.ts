import type { Ornament, OrnamentInput, OrnamentQuery } from '~/types'
import type { ApiClient } from '~/composables/useApi'
import { ApiError } from '~/composables/useApi'
import { seedOrnaments } from '~/data/ornaments'
import { createMockCollection, generateId, simulateLatency } from './mockDb'
import { filterOrnaments } from '~/utils/ornamentFilters'

export interface OrnamentService {
  /** Lists ornaments; filtering and sorting happen on the server. */
  list(query?: OrnamentQuery): Promise<Ornament[]>
  get(id: string): Promise<Ornament | null>
  create(input: OrnamentInput): Promise<Ornament>
  update(id: string, input: OrnamentInput): Promise<Ornament>
  remove(id: string): Promise<void>
}

/** Drops empty values so the query string only contains active filters. */
function toQueryParams(query: OrnamentQuery = {}) {
  return Object.fromEntries(
    Object.entries(query).filter(([, value]) => value !== undefined && value !== ''),
  ) as Record<string, string>
}

export function createApiOrnamentService(api: ApiClient): OrnamentService {
  return {
    list: query => api.get<Ornament[]>('/ornaments', { query: toQueryParams(query) }),
    async get(id) {
      try {
        return await api.get<Ornament>(`/ornaments/${encodeURIComponent(id)}`)
      }
      catch (error) {
        if (error instanceof ApiError && error.status === 404) return null
        throw error
      }
    },
    create: input => api.post<Ornament>('/ornaments', input),
    update: (id, input) => api.put<Ornament>(`/ornaments/${encodeURIComponent(id)}`, input),
    remove: async (id) => { await api.delete<void>(`/ornaments/${encodeURIComponent(id)}`) },
  }
}

export function createMockOrnamentService(): OrnamentService {
  const ornaments = createMockCollection<Ornament>('ornaments', seedOrnaments)

  const ensureSkuFree = (sku: string, exceptId?: string) => {
    if (ornaments.all().some(o => o.sku.toUpperCase() === sku.toUpperCase() && o.id !== exceptId)) {
      throw new ApiError(`An ornament with SKU ${sku.toUpperCase()} already exists`, 409, { sku: 'This SKU is already in use' })
    }
  }

  return {
    list: query => simulateLatency(filterOrnaments(ornaments.all(), query)),

    get: id => simulateLatency(ornaments.all().find(o => o.id === id) ?? null),

    async create(input) {
      ensureSkuFree(input.sku)
      const now = new Date().toISOString()
      const created: Ornament = { ...input, sku: input.sku.toUpperCase(), id: generateId('orn'), createdAt: now, updatedAt: now }
      ornaments.save([created, ...ornaments.all()])
      return simulateLatency(created)
    },

    async update(id, input) {
      const all = ornaments.all()
      const index = all.findIndex(o => o.id === id)
      if (index === -1) throw new ApiError('Ornament not found', 404)
      ensureSkuFree(input.sku, id)
      const updated: Ornament = { ...all[index]!, ...input, sku: input.sku.toUpperCase(), id, updatedAt: new Date().toISOString() }
      all[index] = updated
      ornaments.save(all)
      return simulateLatency(updated)
    },

    async remove(id) {
      ornaments.save(ornaments.all().filter(o => o.id !== id))
      await simulateLatency(null)
    },
  }
}
