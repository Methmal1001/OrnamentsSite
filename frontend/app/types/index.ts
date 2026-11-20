/**
 * Domain types shared by the UI, the dummy data and the API services.
 * They mirror the DTOs the future ASP.NET Core API is expected to return.
 */

// ---------- Users & authentication ----------

export type UserRole = 'customer' | 'admin'

export interface User {
  id: string
  fullName: string
  email: string
  phone: string
  address: string
  role: UserRole
  createdAt: string
}

/** Editable part of a user – matches a future `PUT /users/me` body. */
export interface UserProfile {
  fullName: string
  email: string
  phone: string
  address: string
}

export interface LoginRequest {
  email: string
  password: string
  rememberMe: boolean
}

export interface RegisterRequest {
  fullName: string
  email: string
  phone: string
  password: string
  address: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

/**
 * Signed-in session. With the real API the JWT lives in an HttpOnly cookie that
 * JavaScript cannot read, so the frontend only ever holds the user and expiry.
 */
export interface AuthSession {
  expiresAt: string
  user: User
}

export interface NotificationPreferences {
  emailNotificationsEnabled: boolean
  inAppNotificationsEnabled: boolean
  updatedAt?: string
}

// ---------- Ornaments ----------

export type CategoryId = 'gold-rings' | 'necklaces' | 'bracelets' | 'earrings' | 'pendants'

export interface OrnamentCategory {
  id: CategoryId
  name: string
  description: string
}

export type AvailabilityStatus = 'in-stock' | 'low-stock' | 'out-of-stock' | 'pre-order'

export interface Ornament {
  id: string
  sku: string
  name: string
  categoryId: CategoryId
  description: string
  price: number
  availability: AvailabilityStatus
  metal: string
  weightGrams: number
  gemstone: string | null
  /** Product photo. When empty the UI renders an illustration instead. */
  imageUrl: string | null
  isFeatured: boolean
  createdAt: string
  // Returned by the API (derived availability is still the source for the UI).
  stockQuantity?: number
  isAvailable?: boolean
  updatedAt?: string
}

/** Body for creating/updating an ornament (server sets id/timestamps/stock fields). */
export type OrnamentInput = Omit<Ornament, 'id' | 'createdAt' | 'updatedAt' | 'stockQuantity' | 'isAvailable'>

export type OrnamentSort = 'featured' | 'newest' | 'price-asc' | 'price-desc'

/** Server-side search/filter parameters for GET /api/ornaments. */
export interface OrnamentQuery {
  search?: string
  category?: CategoryId
  availability?: AvailabilityStatus
  sort?: OrnamentSort
}

// ---------- Notifications ----------

export type NotificationType = 'order' | 'promotion' | 'account' | 'system'

export interface AppNotification {
  id: string
  /** Null for a shop-wide announcement (API). */
  userId: string | null
  title: string
  message: string
  type: NotificationType
  isRead: boolean
  isShopWide?: boolean
  createdAt: string
}

export interface CreateNotificationRequest {
  /** A customer id, or `all` to broadcast to every customer. */
  recipient: string
  title: string
  message: string
  type: NotificationType
}

export type NotificationFilter = 'all' | 'unread' | 'read'

// ---------- API ----------

/** Shape of ASP.NET Core ProblemDetails / validation errors. */
export interface ApiProblem {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

// ---------- UI ----------

export type IconName =
  | 'home' | 'gem' | 'bell' | 'user' | 'users' | 'search' | 'close' | 'menu'
  | 'eye' | 'eye-off' | 'trash' | 'edit' | 'plus' | 'check' | 'check-circle'
  | 'arrow-right' | 'arrow-left' | 'chevron-down' | 'chevron-right' | 'phone'
  | 'mail' | 'map-pin' | 'clock' | 'sparkles' | 'shield' | 'gift' | 'logout'
  | 'grid' | 'chart' | 'lock' | 'warning' | 'info' | 'inbox' | 'heart' | 'tag'
  | 'facebook' | 'instagram' | 'youtube' | 'chat'

export interface NavItem {
  label: string
  to: string
  icon: IconName
  badge?: number
}

export type ToastType = 'success' | 'error' | 'info'

export interface Toast {
  id: number
  message: string
  type: ToastType
}
