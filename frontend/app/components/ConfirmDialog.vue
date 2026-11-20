<script setup lang="ts">
withDefaults(defineProps<{
  open: boolean
  title?: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  tone?: 'danger' | 'primary'
  loading?: boolean
}>(), {
  title: 'Are you sure?',
  confirmLabel: 'Confirm',
  cancelLabel: 'Cancel',
  tone: 'danger',
  loading: false,
})

const emit = defineEmits<{ confirm: [], cancel: [] }>()
</script>

<template>
  <AppModal :open="open" :title="title" size="sm" :persistent="loading" @close="emit('cancel')">
    <div class="flex gap-4">
      <span class="grid size-11 shrink-0 place-items-center rounded-full" :class="tone === 'danger' ? 'bg-rose-50 text-rose-600' : 'bg-gold-50 text-gold-700'">
        <AppIcon :name="tone === 'danger' ? 'warning' : 'info'" :size="22" />
      </span>
      <p class="pt-2 text-sm leading-relaxed text-charcoal-soft">{{ message }}</p>
    </div>
    <template #footer>
      <button type="button" class="btn btn-ghost" :disabled="loading" @click="emit('cancel')">{{ cancelLabel }}</button>
      <button type="button" class="btn" :class="tone === 'danger' ? 'btn-danger' : 'btn-primary'" :disabled="loading" @click="emit('confirm')">
        <LoadingIndicator v-if="loading" variant="inline" />
        {{ confirmLabel }}
      </button>
    </template>
  </AppModal>
</template>
