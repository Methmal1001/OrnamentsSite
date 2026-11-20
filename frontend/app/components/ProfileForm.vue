<script setup lang="ts">
import type { UserProfile } from '~/types'

const props = withDefaults(defineProps<{
  initial: UserProfile
  loading?: boolean
  /** Field errors returned by the API (e.g. { email: 'That email is already used…' }). */
  serverErrors?: Record<string, string>
}>(), { loading: false, serverErrors: () => ({}) })

const emit = defineEmits<{ submit: [profile: UserProfile], cancel: [] }>()

const form = reactive<UserProfile>({ ...props.initial })
watch(() => props.initial, value => Object.assign(form, value), { deep: true })

const { errors, validate, validateField, clearErrors } = useFormValidation(form, {
  fullName: [rules.required('Full name'), rules.minLength(3, 'Full name'), rules.maxLength(80, 'Full name')],
  email: [rules.required('Email'), rules.email()],
  phone: [rules.required('Telephone number'), rules.phone()],
  address: [rules.required('Address'), rules.minLength(5, 'Address')],
})

watch(() => props.serverErrors, (fieldErrors) => {
  for (const [field, message] of Object.entries(fieldErrors)) {
    if (field in form) errors[field] = message
  }
})

const emailChanged = computed(() => form.email.trim().toLowerCase() !== props.initial.email.toLowerCase())

function onSubmit() {
  if (!validate()) return
  emit('submit', {
    fullName: form.fullName.trim(),
    email: form.email.trim(),
    phone: form.phone.trim(),
    address: form.address.trim(),
  })
}

function onCancel() {
  Object.assign(form, props.initial)
  clearErrors()
  emit('cancel')
}
</script>

<template>
  <form class="space-y-5" novalidate @submit.prevent="onSubmit">
    <div class="grid gap-5 sm:grid-cols-2">
      <FormInput v-model="form.fullName" label="Full name" autocomplete="name" required :error="errors.fullName" @blur="validateField('fullName')" />
      <FormInput v-model="form.phone" label="Telephone number" type="tel" autocomplete="tel" placeholder="077 123 4567" required :error="errors.phone" @blur="validateField('phone')" />
    </div>
    <FormInput
      v-model="form.email"
      label="Email address"
      type="email"
      autocomplete="email"
      required
      :error="errors.email"
      :hint="emailChanged ? 'Changing your email will require verification once the backend is connected.' : ''"
      @blur="validateField('email')"
    />
    <FormInput v-model="form.address" label="Address" multiline :rows="3" autocomplete="street-address" required :error="errors.address" @blur="validateField('address')" />

    <div class="flex flex-col-reverse gap-3 pt-2 sm:flex-row sm:justify-end">
      <button type="button" class="btn btn-ghost" :disabled="loading" @click="onCancel">Cancel</button>
      <button type="submit" class="btn btn-primary" :disabled="loading">
        <LoadingIndicator v-if="loading" variant="inline" />
        Save changes
      </button>
    </div>
  </form>
</template>
