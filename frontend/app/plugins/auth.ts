/**
 * Restores the session before the first navigation, so route middleware sees the real
 * sign-in state, and signs the user out automatically when the session expires.
 */
export default defineNuxtPlugin({
  name: 'auth',
  async setup(nuxtApp) {
    const { session, restoreSession, expireSession } = useAuth()
    await restoreSession()

    let timer: ReturnType<typeof setTimeout> | undefined
    // setTimeout overflows above ~24.8 days; sessions are at most 7 days.
    const MAX_DELAY = 2_147_000_000

    watch(() => session.value?.expiresAt, (expiresAt) => {
      clearTimeout(timer)
      if (!expiresAt) return
      const delay = Math.min(new Date(expiresAt).getTime() - Date.now(), MAX_DELAY)
      timer = setTimeout(() => nuxtApp.runWithContext(() => expireSession()), Math.max(delay, 0))
    }, { immediate: true })
  },
})
