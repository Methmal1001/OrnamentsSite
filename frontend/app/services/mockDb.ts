/**
 * In-browser "database" backing the mock services.
 * Each collection is seeded from app/data and persisted to localStorage so
 * demo changes survive page reloads. Delete this file once the API is live.
 */
import { readStorage, writeStorage } from '~/utils/storage'

const MOCK_LATENCY_MS = 350

export function simulateLatency<T>(value: T, ms = MOCK_LATENCY_MS): Promise<T> {
  return new Promise(resolve => setTimeout(() => resolve(value), ms))
}

export function generateId(prefix: string): string {
  return `${prefix}-${Date.now().toString(36)}${Math.random().toString(36).slice(2, 6)}`
}

/** Deep copy so callers can never mutate the stored records by accident. */
export function clone<T>(value: T): T {
  return JSON.parse(JSON.stringify(value)) as T
}

export function createMockCollection<T>(key: string, seed: T[]) {
  const storageKey = `db:${key}`
  return {
    all(): T[] {
      return clone(readStorage<T[]>(storageKey, seed))
    },
    save(items: T[]): void {
      writeStorage(storageKey, items)
    },
  }
}
