import type { AppNotification } from '~/types'

export const seedNotifications: AppNotification[] = [
  {
    id: 'ntf-001', userId: 'usr-001', type: 'promotion', isRead: false, createdAt: '2026-11-19T08:30:00Z',
    title: 'Festive Season Sale – 15% off',
    message: 'Enjoy 15% off making charges on all necklaces and bracelets until 31 December.',
  },
  {
    id: 'ntf-002', userId: 'usr-001', type: 'order', isRead: false, createdAt: '2026-11-18T14:10:00Z',
    title: 'Your reservation is ready',
    message: 'The Ceylon Blue Sapphire Solitaire you reserved is ready for viewing at our Colombo showroom.',
  },
  {
    id: 'ntf-003', userId: 'usr-001', type: 'account', isRead: true, createdAt: '2026-11-12T09:00:00Z',
    title: 'Profile updated',
    message: 'Your contact details were updated successfully.',
  },
  {
    id: 'ntf-004', userId: 'usr-001', type: 'system', isRead: false, createdAt: '2026-11-05T16:45:00Z',
    title: 'New collection: Lotus Pendants',
    message: 'Discover our new cat\'s eye and moonstone lotus pendants, handcrafted in Kandy.',
  },
  {
    id: 'ntf-005', userId: 'usr-001', type: 'promotion', isRead: true, createdAt: '2026-10-28T11:20:00Z',
    title: 'Free gold cleaning service',
    message: 'Bring any Lanka Ornaments piece to our showroom for a complimentary cleaning and polish.',
  },
  {
    id: 'ntf-006', userId: 'usr-002', type: 'promotion', isRead: false, createdAt: '2026-11-19T08:30:00Z',
    title: 'Festive Season Sale – 15% off',
    message: 'Enjoy 15% off making charges on all necklaces and bracelets until 31 December.',
  },
]
