<script setup lang="ts">
import type { IconName } from '~/types'

useHead({ title: 'Contact' })

const toast = useToast()
const { user } = useAuth()

const form = reactive({
  name: user.value?.fullName ?? '',
  email: user.value?.email ?? '',
  phone: user.value?.phone ?? '',
  message: '',
})
const sending = ref(false)

const { errors, validate, validateField, clearErrors } = useFormValidation(form, {
  name: [rules.required('Name')],
  email: [rules.required('Email'), rules.email()],
  message: [rules.required('Message'), rules.minLength(10, 'Message')],
})

async function submit() {
  if (!validate()) return
  sending.value = true
  // Simulated submission – will POST to the API in Phase 2.
  await new Promise(resolve => setTimeout(resolve, 600))
  sending.value = false
  form.message = ''
  clearErrors()
  toast.success('Thank you for your message. We will reply within one business day.')
}

const contacts: { icon: IconName, title: string, lines: string[], href?: string }[] = [
  { icon: 'map-pin', title: 'Showroom', lines: ['125 Main Street', 'Colombo 11, Sri Lanka'] },
  { icon: 'phone', title: 'Call us', lines: ['+94 11 234 5678', '+94 77 123 4567'], href: 'tel:+94112345678' },
  { icon: 'mail', title: 'Email', lines: ['hello@lankaornaments.lk'], href: 'mailto:hello@lankaornaments.lk' },
  { icon: 'clock', title: 'Opening hours', lines: ['Mon – Sat: 9.30 am – 7.00 pm', 'Sunday: 10.00 am – 2.00 pm'] },
]
</script>

<template>
  <div>
    <section class="bg-linear-to-br from-cream to-beige">
      <div class="container-page py-14 text-center">
        <p class="eyebrow">We’d love to hear from you</p>
        <h1 class="section-title mt-2 sm:text-5xl">Contact Us</h1>
        <p class="mx-auto mt-3 max-w-xl text-charcoal-soft">Questions about a piece, a custom design or an appointment? Our consultants are here to help.</p>
      </div>
    </section>

    <section class="container-page grid gap-8 py-14 lg:grid-cols-[1fr_1.4fr]">
      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-1">
        <component
          :is="contact.href ? 'a' : 'div'"
          v-for="contact in contacts"
          :key="contact.title"
          :href="contact.href"
          class="card flex gap-4 p-5 transition hover:shadow-lift"
        >
          <span class="grid size-11 shrink-0 place-items-center rounded-full bg-gold-50 text-gold-600"><AppIcon :name="contact.icon" /></span>
          <div>
            <h2 class="font-sans text-sm font-semibold">{{ contact.title }}</h2>
            <p v-for="line in contact.lines" :key="line" class="text-sm text-charcoal-soft">{{ line }}</p>
          </div>
        </component>
      </div>

      <form class="card space-y-5 p-6 sm:p-8" novalidate @submit.prevent="submit">
        <h2 class="text-3xl font-semibold">Send us a message</h2>
        <div class="grid gap-5 sm:grid-cols-2">
          <FormInput v-model="form.name" label="Your name" autocomplete="name" required :error="errors.name" @blur="validateField('name')" />
          <FormInput v-model="form.email" label="Email" type="email" autocomplete="email" required :error="errors.email" @blur="validateField('email')" />
        </div>
        <FormInput v-model="form.phone" label="Telephone (optional)" type="tel" autocomplete="tel" />
        <FormInput v-model="form.message" label="Message" multiline :rows="5" required :error="errors.message" @blur="validateField('message')" />
        <button type="submit" class="btn btn-primary w-full py-3 sm:w-auto sm:px-10" :disabled="sending">
          <LoadingIndicator v-if="sending" variant="inline" />
          Send message
        </button>
      </form>
    </section>
  </div>
</template>
