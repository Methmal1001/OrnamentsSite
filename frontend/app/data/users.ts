import type { User } from '~/types'

/**
 * Demo accounts. Passwords are stored in plain text ONLY because this is a
 * frontend-only demo. Real credential handling belongs to the backend.
 */
export interface DemoUserRecord extends User {
  password: string
}

export const DEMO_CUSTOMER = { email: 'customer@lankaornaments.lk', password: 'Customer@123' } as const
export const DEMO_ADMIN = { email: 'admin@lankaornaments.lk', password: 'Admin@123' } as const

export const seedUsers: DemoUserRecord[] = [
  {
    id: 'usr-001', fullName: 'Nethmi Perera', email: DEMO_CUSTOMER.email, password: DEMO_CUSTOMER.password,
    phone: '0771234567', address: '42 Galle Road, Colombo 03', role: 'customer', createdAt: '2026-03-12T10:00:00Z',
  },
  {
    id: 'usr-002', fullName: 'Kasun Fernando', email: 'kasun@example.lk', password: 'Customer@123',
    phone: '0719876543', address: '18 Temple Street, Kandy', role: 'customer', createdAt: '2026-05-02T10:00:00Z',
  },
  {
    id: 'usr-admin', fullName: 'Store Administrator', email: DEMO_ADMIN.email, password: DEMO_ADMIN.password,
    phone: '0112345678', address: 'Lanka Ornaments, 125 Main Street, Colombo 11', role: 'admin', createdAt: '2026-01-01T10:00:00Z',
  },
]
