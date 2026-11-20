// https://nuxt.com/docs/api/configuration/nuxt-config
import tailwindcss from '@tailwindcss/vite'

export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  // Client-rendered SPA: the API (cookie session) is the source of truth and pages are
  // personalised per user. The Nitro server only serves static assets.
  ssr: false,

  css: ['~/assets/css/main.css'],

  vite: {
    plugins: [tailwindcss()],
  },

  runtimeConfig: {
    public: {
      // Overridable at runtime with NUXT_PUBLIC_API_BASE_URL (use an https:// URL when deployed).
      apiBaseUrl: 'http://localhost:5196/api',
      // NUXT_PUBLIC_USE_MOCK_API=true runs the offline demo with local dummy data.
      useMockApi: false,
      // NUXT_PUBLIC_SHOW_DEMO_ACCOUNTS=true shows test-account shortcuts on the login page (dev only).
      showDemoAccounts: false,
    },
  },

  app: {
    head: {
      htmlAttrs: { lang: 'en' },
      title: 'Lanka Ornaments',
      titleTemplate: '%s · Lanka Ornaments',
      meta: [
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        {
          name: 'description',
          content: 'Lanka Ornaments – handcrafted Sri Lankan gold jewellery, sapphires and fine ornaments.',
        },
        { name: 'theme-color', content: '#c39a52' },
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Cormorant+Garamond:wght@400;500;600;700&family=Jost:wght@300;400;500;600&display=swap',
        },
      ],
    },
  },

  typescript: {
    strict: true,
  },
})
