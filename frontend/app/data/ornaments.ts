import type { Ornament } from '~/types'

/** Seed catalogue used by the mock ornament service. */
export const seedOrnaments: Ornament[] = [
  {
    id: 'orn-001', sku: 'LO-RG-001', name: 'Ceylon Blue Sapphire Solitaire', categoryId: 'gold-rings',
    description: 'A 1.2 carat cornflower-blue Ceylon sapphire set in a polished 18K yellow-gold band with a delicate cathedral setting.',
    price: 385000, availability: 'in-stock', metal: '18K Yellow Gold', weightGrams: 4.2, gemstone: 'Blue Sapphire',
    imageUrl: null, isFeatured: true, createdAt: '2026-10-28T09:00:00Z',
  },
  {
    id: 'orn-002', sku: 'LO-RG-002', name: 'Classic 22K Wedding Band', categoryId: 'gold-rings',
    description: 'A comfort-fit 22K gold band with a softly brushed centre and high-polish edges. Made to be worn every day.',
    price: 118000, availability: 'in-stock', metal: '22K Yellow Gold', weightGrams: 5.8, gemstone: null,
    imageUrl: null, isFeatured: false, createdAt: '2026-09-14T09:00:00Z',
  },
  {
    id: 'orn-003', sku: 'LO-RG-003', name: 'Padparadscha Halo Ring', categoryId: 'gold-rings',
    description: 'A rare lotus-pink padparadscha sapphire framed by a halo of brilliant-cut diamonds in rose gold.',
    price: 645000, availability: 'low-stock', metal: '18K Rose Gold', weightGrams: 4.9, gemstone: 'Padparadscha',
    imageUrl: null, isFeatured: true, createdAt: '2026-11-10T09:00:00Z',
  },
  {
    id: 'orn-004', sku: 'LO-NK-001', name: 'Kandyan Bridal Necklace', categoryId: 'necklaces',
    description: 'A traditional Kandyan-style bridal necklace with hand-chased gold medallions and ruby accents.',
    price: 1250000, availability: 'pre-order', metal: '22K Yellow Gold', weightGrams: 64, gemstone: 'Ruby',
    imageUrl: null, isFeatured: true, createdAt: '2026-08-02T09:00:00Z',
  },
  {
    id: 'orn-005', sku: 'LO-NK-002', name: 'Freshwater Pearl Strand', categoryId: 'necklaces',
    description: 'Lustrous hand-knotted freshwater pearls finished with an 18K gold filigree clasp.',
    price: 98000, availability: 'in-stock', metal: '18K Yellow Gold', weightGrams: 22, gemstone: 'Pearl',
    imageUrl: null, isFeatured: false, createdAt: '2026-10-05T09:00:00Z',
  },
  {
    id: 'orn-006', sku: 'LO-NK-003', name: 'Emerald Drop Choker', categoryId: 'necklaces',
    description: 'A slim gold choker holding a single pear-shaped emerald for a modern, elegant look.',
    price: 312000, availability: 'out-of-stock', metal: '18K Yellow Gold', weightGrams: 11.5, gemstone: 'Emerald',
    imageUrl: null, isFeatured: false, createdAt: '2026-07-21T09:00:00Z',
  },
  {
    id: 'orn-007', sku: 'LO-BR-001', name: 'Diamond Tennis Bracelet', categoryId: 'bracelets',
    description: 'Forty brilliant-cut diamonds in a flexible white-gold line bracelet with a hidden safety clasp.',
    price: 890000, availability: 'low-stock', metal: '18K White Gold', weightGrams: 12.3, gemstone: 'Diamond',
    imageUrl: null, isFeatured: true, createdAt: '2026-11-02T09:00:00Z',
  },
  {
    id: 'orn-008', sku: 'LO-BR-002', name: 'Twisted Rope Bangle', categoryId: 'bracelets',
    description: 'A solid 22K gold bangle with a hand-twisted rope texture that catches the light beautifully.',
    price: 236000, availability: 'in-stock', metal: '22K Yellow Gold', weightGrams: 15.6, gemstone: null,
    imageUrl: null, isFeatured: false, createdAt: '2026-09-30T09:00:00Z',
  },
  {
    id: 'orn-009', sku: 'LO-BR-003', name: 'Moonstone Charm Bracelet', categoryId: 'bracelets',
    description: 'Dainty gold chain with Sri Lankan moonstone charms that glow with a soft blue sheen.',
    price: 74500, availability: 'in-stock', metal: '18K Yellow Gold', weightGrams: 6.1, gemstone: 'Moonstone',
    imageUrl: null, isFeatured: false, createdAt: '2026-11-15T09:00:00Z',
  },
  {
    id: 'orn-010', sku: 'LO-ER-001', name: 'Sapphire Jhumka Earrings', categoryId: 'earrings',
    description: 'Bell-shaped jhumkas in 22K gold with blue sapphire drops and fine granulation work.',
    price: 168000, availability: 'in-stock', metal: '22K Yellow Gold', weightGrams: 9.4, gemstone: 'Blue Sapphire',
    imageUrl: null, isFeatured: true, createdAt: '2026-10-19T09:00:00Z',
  },
  {
    id: 'orn-011', sku: 'LO-ER-002', name: 'Pearl Drop Earrings', categoryId: 'earrings',
    description: 'Elegant South Sea pearls suspended from polished gold hooks. Timeless for any occasion.',
    price: 64000, availability: 'in-stock', metal: '18K Yellow Gold', weightGrams: 3.8, gemstone: 'Pearl',
    imageUrl: null, isFeatured: false, createdAt: '2026-08-25T09:00:00Z',
  },
  {
    id: 'orn-012', sku: 'LO-ER-003', name: 'Ruby Cluster Studs', categoryId: 'earrings',
    description: 'Flower-shaped clusters of vivid rubies in a secure screw-back setting.',
    price: 142000, availability: 'out-of-stock', metal: '18K Rose Gold', weightGrams: 3.2, gemstone: 'Ruby',
    imageUrl: null, isFeatured: false, createdAt: '2026-06-11T09:00:00Z',
  },
  {
    id: 'orn-013', sku: 'LO-PD-001', name: 'Cat\'s Eye Lotus Pendant', categoryId: 'pendants',
    description: 'A chrysoberyl cat\'s eye cabochon in a lotus-petal gold frame, symbolising purity and protection.',
    price: 205000, availability: 'in-stock', metal: '22K Yellow Gold', weightGrams: 5.4, gemstone: 'Cat\'s Eye',
    imageUrl: null, isFeatured: true, createdAt: '2026-11-18T09:00:00Z',
  },
  {
    id: 'orn-014', sku: 'LO-PD-002', name: 'Yellow Sapphire Teardrop', categoryId: 'pendants',
    description: 'A sunny yellow sapphire teardrop in a minimalist bezel, on an 18-inch gold chain.',
    price: 158000, availability: 'low-stock', metal: '18K Yellow Gold', weightGrams: 4.1, gemstone: 'Yellow Sapphire',
    imageUrl: null, isFeatured: false, createdAt: '2026-10-01T09:00:00Z',
  },
  {
    id: 'orn-015', sku: 'LO-PD-003', name: 'Diamond Initial Pendant', categoryId: 'pendants',
    description: 'A personalised pavé-diamond initial pendant – a meaningful everyday keepsake.',
    price: 92000, availability: 'pre-order', metal: '18K White Gold', weightGrams: 2.6, gemstone: 'Diamond',
    imageUrl: null, isFeatured: false, createdAt: '2026-11-08T09:00:00Z',
  },
  {
    id: 'orn-016', sku: 'LO-RG-004', name: 'Emerald Eternity Ring', categoryId: 'gold-rings',
    description: 'A full eternity band of calibré-cut emeralds in 18K yellow gold.',
    price: 274000, availability: 'in-stock', metal: '18K Yellow Gold', weightGrams: 3.6, gemstone: 'Emerald',
    imageUrl: null, isFeatured: false, createdAt: '2026-07-04T09:00:00Z',
  },
]
