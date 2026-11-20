<script setup lang="ts">
import type { UserProfile } from '~/types'

definePageMeta({ layout: 'dashboard', middleware: 'auth' })
useHead({ title: 'My Profile' })

const { user, updateProfile, changePassword, refreshProfile } = useAuth()
const toast = useToast()

const editing = ref(false)
const savingProfile = ref(false)
const profileServerErrors = ref<Record<string, string>>({})
const loadingProfile = ref(true)
const profileLoadError = ref<string | null>(null)

// Always show what the server has (GET /api/profile/me), not only the cached session copy.
async function loadProfile() {
  loadingProfile.value = true
  profileLoadError.value = null
  try {
    await refreshProfile()
  }
  catch (error) {
    profileLoadError.value = errorMessage(error, 'Could not load your profile.')
  }
  finally {
    loadingProfile.value = false
  }
}
loadProfile()

const profile = computed<UserProfile>(() => ({
  fullName: user.value?.fullName ?? '',
  email: user.value?.email ?? '',
  phone: user.value?.phone ?? '',
  address: user.value?.address ?? '',
}))

async function saveProfile(next: UserProfile) {
  savingProfile.value = true
  profileServerErrors.value = {}
  try {
    const emailChanged = next.email.toLowerCase() !== profile.value.email.toLowerCase()
    await updateProfile(next)
    editing.value = false
    toast.success(emailChanged ? 'Profile saved. Your new email address will need to be verified.' : 'Your profile has been updated.')
  }
  catch (error) {
    // Backend validation (400) and email conflicts (409) are shown next to the fields.
    if (error instanceof ApiError && error.status === 409) profileServerErrors.value = { email: error.message }
    else if (error instanceof ApiError && Object.keys(error.fieldErrors).length) profileServerErrors.value = { ...error.fieldErrors }
    else toast.error(errorMessage(error))
  }
  finally {
    savingProfile.value = false
  }
}

// ----- Change password -----
const passwordForm = reactive({ currentPassword: '', newPassword: '', confirmPassword: '' })
const savingPassword = ref(false)
const {
  errors: passwordErrors, validate: validatePassword, validateField: validatePasswordField,
  clearErrors: clearPasswordErrors, applyServerErrors: applyPasswordServerErrors,
} = useFormValidation(passwordForm, {
  currentPassword: [rules.required('Current password')],
  newPassword: [
    rules.required('New password'),
    rules.strongPassword(),
    value => value !== passwordForm.currentPassword || 'New password must be different from the current one',
  ],
  confirmPassword: [rules.required('Please confirm the new password'), rules.sameAs(() => passwordForm.newPassword, 'Passwords do not match')],
})

async function submitPassword() {
  if (!validatePassword()) return
  savingPassword.value = true
  try {
    await changePassword({ currentPassword: passwordForm.currentPassword, newPassword: passwordForm.newPassword })
    Object.assign(passwordForm, { currentPassword: '', newPassword: '', confirmPassword: '' })
    clearPasswordErrors()
    toast.success('Your password has been changed.')
  }
  catch (error) {
    if (!applyPasswordServerErrors(error)) toast.error(errorMessage(error))
  }
  finally {
    savingPassword.value = false
  }
}
</script>

<template>
  <div v-if="user" class="space-y-8">
    <header>
      <p class="eyebrow">Account settings</p>
      <h1 class="section-title mt-1">My Profile</h1>
    </header>

    <section class="card overflow-hidden">
      <div class="flex flex-col gap-6 border-b border-gold-100 bg-linear-to-r from-gold-50 to-ivory p-6 sm:flex-row sm:items-center sm:justify-between">
        <div class="flex items-center gap-4">
          <span class="grid size-16 place-items-center rounded-full bg-linear-to-br from-gold-300 to-gold-600 font-serif text-2xl font-semibold text-white shadow-soft">{{ initials(user.fullName) }}</span>
          <div>
            <h2 class="text-2xl font-semibold">{{ user.fullName }}</h2>
            <p class="text-sm text-charcoal-soft">Customer · Member since {{ formatDate(user.createdAt) }}</p>
          </div>
        </div>
        <button v-if="!editing && !loadingProfile && !profileLoadError" type="button" class="btn btn-outline" @click="editing = true">
          <AppIcon name="edit" :size="16" /> Edit profile
        </button>
      </div>

      <div class="p-6">
        <LoadingIndicator v-if="loadingProfile" label="Loading your profile…" />
        <EmptyState v-else-if="profileLoadError" icon="warning" title="Unable to load your profile" :description="profileLoadError">
          <button type="button" class="btn btn-primary" @click="loadProfile">Try again</button>
        </EmptyState>
        <ProfileForm
          v-else-if="editing"
          :initial="profile"
          :loading="savingProfile"
          :server-errors="profileServerErrors"
          @submit="saveProfile"
          @cancel="editing = false"
        />
        <dl v-else class="grid gap-6 sm:grid-cols-2">
          <div>
            <dt class="text-xs font-medium uppercase tracking-wider text-charcoal-soft">Full name</dt>
            <dd class="mt-1 font-medium">{{ user.fullName }}</dd>
          </div>
          <div>
            <dt class="text-xs font-medium uppercase tracking-wider text-charcoal-soft">Email</dt>
            <dd class="mt-1 flex flex-wrap items-center gap-2 font-medium">
              {{ user.email }}
              <span class="badge bg-amber-50 text-amber-700 ring-1 ring-amber-200">Email verification coming soon</span>
            </dd>
          </div>
          <div>
            <dt class="text-xs font-medium uppercase tracking-wider text-charcoal-soft">Telephone</dt>
            <dd class="mt-1 font-medium">{{ user.phone }}</dd>
          </div>
          <div>
            <dt class="text-xs font-medium uppercase tracking-wider text-charcoal-soft">Address</dt>
            <dd class="mt-1 font-medium">{{ user.address }}</dd>
          </div>
        </dl>
      </div>
    </section>

    <section class="card p-6">
      <div class="mb-6 flex items-start gap-4">
        <span class="grid size-11 shrink-0 place-items-center rounded-full bg-gold-50 text-gold-600"><AppIcon name="lock" /></span>
        <div>
          <h2 class="text-2xl font-semibold">Change password</h2>
          <p class="text-sm text-charcoal-soft">Use at least 8 characters with upper- and lower-case letters and a number.</p>
        </div>
      </div>
      <form class="grid max-w-xl gap-5" novalidate @submit.prevent="submitPassword">
        <FormInput v-model="passwordForm.currentPassword" label="Current password" type="password" autocomplete="current-password" required :error="passwordErrors.currentPassword" @blur="validatePasswordField('currentPassword')" />
        <FormInput v-model="passwordForm.newPassword" label="New password" type="password" autocomplete="new-password" required :error="passwordErrors.newPassword" @blur="validatePasswordField('newPassword')" />
        <FormInput v-model="passwordForm.confirmPassword" label="Confirm new password" type="password" autocomplete="new-password" required :error="passwordErrors.confirmPassword" @blur="validatePasswordField('confirmPassword')" />
        <div>
          <button type="submit" class="btn btn-primary" :disabled="savingPassword">
            <LoadingIndicator v-if="savingPassword" variant="inline" />
            Update password
          </button>
        </div>
      </form>
    </section>
  </div>
</template>
