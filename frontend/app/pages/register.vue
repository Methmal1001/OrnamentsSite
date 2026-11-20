<script setup lang="ts">
definePageMeta({ layout: 'auth', middleware: 'guest' })
useHead({ title: 'Register' })

const { register } = useAuth()
const toast = useToast()

const form = reactive({
  fullName: '',
  email: '',
  phone: '',
  password: '',
  confirmPassword: '',
  address: '',
  acceptTerms: false,
})
const submitting = ref(false)
const formError = ref('')

const { errors, validate, validateField, applyServerErrors } = useFormValidation(form, {
  fullName: [rules.required('Full name'), rules.minLength(3, 'Full name')],
  email: [rules.required('Email'), rules.email()],
  phone: [rules.required('Telephone number'), rules.phone()],
  password: [rules.required('Password'), rules.strongPassword()],
  confirmPassword: [rules.required('Please confirm your password'), rules.sameAs(() => form.password, 'Passwords do not match')],
  address: [rules.required('Address'), rules.minLength(5, 'Address')],
  acceptTerms: [value => value === true || 'Please accept the terms to continue'],
})

// Re-check the confirmation when the password changes after it was entered.
watch(() => form.password, () => {
  if (form.confirmPassword) validateField('confirmPassword')
})

const strength = computed(() => {
  const p = form.password
  const score = [p.length >= 8, /[A-Z]/.test(p) && /[a-z]/.test(p), /\d/.test(p), /[^A-Za-z0-9]/.test(p)].filter(Boolean).length
  return { score, label: ['Too weak', 'Weak', 'Fair', 'Good', 'Strong'][score] ?? '' }
})

async function submit() {
  formError.value = ''
  if (!validate()) return
  submitting.value = true
  try {
    const user = await register({
      fullName: form.fullName,
      email: form.email,
      phone: form.phone,
      password: form.password,
      address: form.address,
    })
    toast.success(`Welcome to Lanka Ornaments, ${user.fullName.split(' ')[0]}!`)
    await navigateTo('/dashboard')
  }
  catch (error) {
    if (error instanceof ApiError && error.status === 409) {
      errors.email = error.message // duplicate account
    }
    else if (!applyServerErrors(error)) {
      formError.value = errorMessage(error)
    }
  }
  finally {
    submitting.value = false
  }
}
</script>

<template>
  <div>
    <p class="eyebrow">Join us</p>
    <h1 class="mt-2 text-4xl font-semibold">Create your account</h1>
    <p class="mt-2 text-sm text-charcoal-soft">
      Already registered?
      <NuxtLink to="/login" class="font-medium text-gold-700 underline-offset-4 hover:underline">Sign in</NuxtLink>
    </p>

    <form class="mt-8 space-y-5" novalidate @submit.prevent="submit">
      <p v-if="formError" class="flex items-center gap-2 rounded-xl bg-rose-50 px-4 py-3 text-sm text-rose-700" role="alert">
        <AppIcon name="warning" :size="16" /> {{ formError }}
      </p>

      <FormInput v-model="form.fullName" label="Full name" autocomplete="name" required :error="errors.fullName" @blur="validateField('fullName')" />
      <div class="grid gap-5 sm:grid-cols-2">
        <FormInput v-model="form.email" label="Email" type="email" autocomplete="email" required :error="errors.email" @blur="validateField('email')" />
        <FormInput v-model="form.phone" label="Telephone number" type="tel" autocomplete="tel" placeholder="077 123 4567" required :error="errors.phone" @blur="validateField('phone')" />
      </div>

      <div>
        <FormInput v-model="form.password" label="Password" type="password" autocomplete="new-password" required :error="errors.password" hint="At least 8 characters with upper- and lower-case letters and a number." @blur="validateField('password')" />
        <div v-if="form.password" class="mt-2 flex items-center gap-2">
          <div class="flex flex-1 gap-1">
            <span v-for="n in 4" :key="n" class="h-1 flex-1 rounded-full transition-colors" :class="n <= strength.score ? (strength.score >= 3 ? 'bg-emerald-500' : 'bg-amber-400') : 'bg-gold-100'" />
          </div>
          <span class="text-xs text-charcoal-soft">{{ strength.label }}</span>
        </div>
      </div>
      <FormInput v-model="form.confirmPassword" label="Confirm password" type="password" autocomplete="new-password" required :error="errors.confirmPassword" @blur="validateField('confirmPassword')" />
      <FormInput v-model="form.address" label="Address" multiline :rows="2" autocomplete="street-address" required :error="errors.address" @blur="validateField('address')" />

      <div>
        <label class="flex cursor-pointer items-start gap-2 text-sm">
          <input v-model="form.acceptTerms" type="checkbox" class="mt-0.5 size-4 rounded accent-gold-600" @change="validateField('acceptTerms')">
          <span>I agree to the terms of service and privacy policy.</span>
        </label>
        <p v-if="errors.acceptTerms" class="mt-1.5 text-xs text-rose-600">{{ errors.acceptTerms }}</p>
      </div>

      <button type="submit" class="btn btn-primary w-full py-3" :disabled="submitting">
        <LoadingIndicator v-if="submitting" variant="inline" />
        {{ submitting ? 'Creating account…' : 'Register' }}
      </button>
    </form>
  </div>
</template>
