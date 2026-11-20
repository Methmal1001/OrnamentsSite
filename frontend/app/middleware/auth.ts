/** Customer area guard (UI convenience only – the API must enforce access). */
export default defineNuxtRouteMiddleware((to) => {
  const { isAuthenticated, isAdmin } = useAuth()
  if (!isAuthenticated.value) {
    return navigateTo({ path: '/login', query: { redirect: to.fullPath } })
  }
  if (isAdmin.value) {
    return navigateTo('/admin')
  }
})
