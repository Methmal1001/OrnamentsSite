/**
 * Thin, failure-tolerant wrappers around Web Storage.
 * Used only for temporary demo data in Phase 1.
 */
const PREFIX = 'lanka-ornaments:'

type StorageKind = 'local' | 'session'

function getStore(kind: StorageKind): Storage | null {
  if (!import.meta.client) return null
  try {
    return kind === 'local' ? window.localStorage : window.sessionStorage
  }
  catch {
    return null
  }
}

export function readStorage<T>(key: string, fallback: T, kind: StorageKind = 'local'): T {
  try {
    const raw = getStore(kind)?.getItem(PREFIX + key)
    return raw ? (JSON.parse(raw) as T) : fallback
  }
  catch {
    return fallback
  }
}

export function writeStorage<T>(key: string, value: T, kind: StorageKind = 'local'): void {
  try {
    getStore(kind)?.setItem(PREFIX + key, JSON.stringify(value))
  }
  catch {
    // Storage full or blocked: the demo keeps working in memory.
  }
}

export function removeStorage(key: string, kind: StorageKind = 'local'): void {
  try {
    getStore(kind)?.removeItem(PREFIX + key)
  }
  catch {
    // ignore
  }
}

/** Removes every key written by this demo. */
export function clearDemoStorage(): void {
  for (const kind of ['local', 'session'] as const) {
    const store = getStore(kind)
    if (!store) continue
    Object.keys(store)
      .filter(key => key.startsWith(PREFIX))
      .forEach(key => store.removeItem(key))
  }
}
