import type { Toast, ToastType } from '~/types'

let nextId = 1

export function useToast() {
  const toasts = useState<Toast[]>('toasts', () => [])

  function dismiss(id: number) {
    toasts.value = toasts.value.filter(t => t.id !== id)
  }

  function show(message: string, type: ToastType = 'success', duration = 3500) {
    // The same message may be raised by several places (e.g. an expired session); show it once.
    if (!message || toasts.value.some(t => t.message === message)) return
    const id = nextId++
    toasts.value = [...toasts.value, { id, message, type }]
    setTimeout(() => dismiss(id), duration)
  }

  return {
    toasts,
    dismiss,
    success: (message: string) => show(message, 'success'),
    error: (message: string) => show(message, 'error', 5000),
    info: (message: string) => show(message, 'info', 4500),
  }
}
