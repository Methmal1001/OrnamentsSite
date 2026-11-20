<script setup lang="ts">
definePageMeta({ layout: 'auth', middleware: 'guest' })
useHead({ title: 'Forgot password' })

const form = reactive({ email: '' })
const sending = ref(false)
const sent = ref(false)

const { errors, validate, validateField } = useFormValidation(form, {
  email: [rules.required('Email'), rules.email()],
})

async function submit() {
  if (!validate()) return
  sending.value = true
  // Simulated – the backend will send a real reset email in Phase 2.
  await new Promise(resolve => setTimeout(resolve, 600))
  sending.value = false
  sent.value = true
}
</script>

<template>
  <div>
    <template v-if="!sent">
      <p class="eyebrow">Account recovery</p>
      <h1 class="mt-2 text-4xl font-semibold">Forgot your password?</h1>
      <p class="mt-2 text-sm text-charcoal-soft">Enter the email address you registered with and we’ll send you a reset link.</p>

      <form class="mt-8 space-y-5" novalidate @submit.prevent="submit">
        <FormInput v-model="form.email" label="Email address" type="email" autocomplete="email" required :error="errors.email" @blur="validateField('email')" />
        <button type="submit" class="btn btn-primary w-full py-3" :disabled="sending">
          <LoadingIndicator v-if="sending" variant="inline" />
          Send reset link
        </button>
      </form>
    </template>

    <div v-else class="text-center">
      <span class="mx-auto grid size-16 place-items-center rounded-full bg-emerald-50 text-emerald-600"><AppIcon name="mail" :size="28" /></span>
      <h1 class="mt-6 text-4xl font-semibold">Check your inbox</h1>
      <p class="mt-3 text-sm text-charcoal-soft">
        If an account exists for <strong>{{ form.email }}</strong>, a password reset link will be sent.
        (Demo only: no email is actually sent.)
      </p>
    </div>

    <NuxtLink to="/login" class="btn btn-ghost mt-6 w-full"><AppIcon name="arrow-left" :size="16" /> Back to login</NuxtLink>
  </div>
</template>
