/** Keeps signed-in users away from the login / register pages. */
export default defineNuxtRouteMiddleware(() => {
  const { isAuthenticated, homeRoute } = useAuth()
  if (isAuthenticated.value) {
    return navigateTo(homeRoute.value)
  }
})
