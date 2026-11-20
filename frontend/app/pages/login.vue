<script setup lang="ts">
import type { LoginRequest } from '~/types'
import { DEMO_ADMIN, DEMO_CUSTOMER } from '~/data/users'

definePageMeta({ layout: 'auth', middleware: 'guest' })
useHead({ title: 'Login' })

const { login } = useAuth()
const toast = useToast()
const route = useRoute()
const { useMockApi: mockMode, showDemoAccounts: demoFlag } = useRuntimeConfig().public
// Test credentials are never shown in production unless explicitly enabled.
const showDemoAccounts = mockMode || demoFlag

const form = reactive<LoginRequest>({ email: '', password: '', rememberMe: false })
const submitting = ref(false)
const formError = ref('')

const { errors, validate, validateField, applyServerErrors } = useFormValidation(form, {
  email: [rules.required('Email'), rules.email()],
  password: [rules.required('Password')],
})

function fillDemo(account: { email: string, password: string }) {
  form.email = account.email
  form.password = account.password
  formError.value = ''
}

async function submit() {
  formError.value = ''
  if (!validate()) return
  submitting.value = true
  try {
    const user = await login(form)
    toast.success(`Welcome back, ${user.fullName.split(' ')[0]}!`)
    const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') && !route.query.redirect.startsWith('//') ? route.query.redirect : null
    await navigateTo(redirect && !(user.role === 'customer' && redirect.startsWith('/admin')) ? redirect : user.role === 'admin' ? '/admin' : '/dashboard')
  }
  catch (error) {
    // Field problems go next to the inputs; everything else (wrong password, lockout, network) above the form.
    if (!applyServerErrors(error)) formError.value = errorMessage(error)
  }
  finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <p class="eyebrow">Welcome back</p>
    <h1 class="mt-2 text-4xl font-semibold">Sign in to your account</h1>
    <p class="mt-2 text-sm text-charcoal-soft">
      New to Lanka Ornaments?
      <NuxtLink to="/register" class="font-medium text-gold-700 underline-offset-4 hover:underline">Create an account</NuxtLink>
    </p>

    <div v-if="showDemoAccounts" class="mt-6 rounded-2xl border border-amber-200 bg-amber-50/70 p-4 text-sm">
      <p class="flex items-center gap-2 font-medium text-amber-800">
        <AppIcon name="info" :size="16" /> {{ mockMode ? 'Demo mode – simulated login only' : 'Test accounts (development only)' }}
      </p>
      <p class="mt-1 text-xs text-amber-800/80">
        {{ mockMode ? 'No real security is provided. Use a demo account:' : 'Fill in a seeded test account:' }}
      </p>
      <div class="mt-3 flex flex-wrap gap-2">
        <button type="button" class="btn btn-outline btn-sm bg-white" @click="fillDemo(DEMO_CUSTOMER)"><AppIcon name="user" :size="14" /> Customer demo</button>
        <button type="button" class="btn btn-outline btn-sm bg-white" @click="fillDemo(DEMO_ADMIN)"><AppIcon name="shield" :size="14" /> Admin demo</button>
      </div>
    </div>

    <form class="mt-6 space-y-5" novalidate @submit.prevent="submit">
      <p v-if="formError" class="flex items-center gap-2 rounded-xl bg-rose-50 px-4 py-3 text-sm text-rose-700" role="alert">
        <AppIcon name="warning" :size="16" /> {{ formError }}
      </p>

      <FormInput v-model="form.email" label="Email address" type="email" autocomplete="email" placeholder="you@example.com" required :error="errors.email" @blur="validateField('email')" />
      <FormInput v-model="form.password" label="Password" type="password" autocomplete="current-password" placeholder="••••••••" required :error="errors.password" @blur="validateField('password')" />

      <div class="flex items-center justify-between gap-4 text-sm">
        <label class="flex cursor-pointer items-center gap-2">
          <input v-model="form.rememberMe" type="checkbox" class="size-4 rounded accent-gold-600">
          Remember me
        </label>
        <NuxtLink to="/forgot-password" class="font-medium text-gold-700 hover:underline">Forgot password?</NuxtLink>
      </div>

      <button type="submit" class="btn btn-primary w-full py-3" :disabled="submitting">
        <LoadingIndicator v-if="submitting" variant="inline" />
        {{ submitting ? 'Signing in…' : 'Login' }}
      </button>
    </form>
  </div>
</template>
