<script setup lang="ts">
import type { NuxtError } from '#app'

const props = defineProps<{ error: NuxtError }>()
const notFound = computed(() => props.error.statusCode === 404)

useHead({ title: notFound.value ? 'Page not found' : 'Something went wrong' })
</script>

<template>
  <div class="flex min-h-screen flex-col bg-ivory">
    <AppNavbar />
    <main class="container-page grid flex-1 place-items-center py-20">
      <EmptyState
        :icon="notFound ? 'search' : 'warning'"
        :title="notFound ? 'We couldn’t find that page' : 'Something went wrong'"
        :description="notFound ? 'The page you are looking for may have moved or no longer exists.' : error.message"
      >
        <button type="button" class="btn btn-primary" @click="clearError({ redirect: '/' })">Back to home</button>
        <button type="button" class="btn btn-outline" @click="clearError({ redirect: '/ornaments' })">Browse ornaments</button>
      </EmptyState>
    </main>
    <AppFooter />
    <AppToast />
  </div>
</template>
