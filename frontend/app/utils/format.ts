import type { AvailabilityStatus, CategoryId, NotificationType } from '~/types'
import { categories } from '~/data/categories'

const priceFormatter = new Intl.NumberFormat('en-LK', { maximumFractionDigits: 0 })

export function formatPrice(value: number): string {
  return `Rs. ${priceFormatter.format(value)}`
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString('en-GB', {
    day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
  })
}

export function formatRelativeTime(iso: string): string {
  const diffMinutes = Math.round((Date.now() - new Date(iso).getTime()) / 60000)
  if (diffMinutes < 1) return 'Just now'
  if (diffMinutes < 60) return `${diffMinutes} min ago`
  const hours = Math.round(diffMinutes / 60)
  if (hours < 24) return `${hours} hour${hours === 1 ? '' : 's'} ago`
  const days = Math.round(hours / 24)
  if (days < 7) return `${days} day${days === 1 ? '' : 's'} ago`
  return formatDate(iso)
}

export function categoryName(id: CategoryId): string {
  return categories.find(c => c.id === id)?.name ?? id
}

export function initials(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part.charAt(0).toUpperCase())
    .join('')
}

export const availabilityMeta: Record<AvailabilityStatus, { label: string, classes: string }> = {
  'in-stock': { label: 'In Stock', classes: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-200' },
  'low-stock': { label: 'Few Left', classes: 'bg-amber-50 text-amber-700 ring-1 ring-amber-200' },
  'out-of-stock': { label: 'Out of Stock', classes: 'bg-rose-50 text-rose-700 ring-1 ring-rose-200' },
  'pre-order': { label: 'Made to Order', classes: 'bg-sky-50 text-sky-700 ring-1 ring-sky-200' },
}

export const notificationTypeMeta: Record<NotificationType, { label: string, classes: string }> = {
  order: { label: 'Order', classes: 'bg-sky-50 text-sky-700' },
  promotion: { label: 'Promotion', classes: 'bg-gold-100 text-gold-800' },
  account: { label: 'Account', classes: 'bg-emerald-50 text-emerald-700' },
  system: { label: 'Announcement', classes: 'bg-violet-50 text-violet-700' },
}
