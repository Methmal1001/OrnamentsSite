<script setup lang="ts">
const { toasts, dismiss } = useToast()

const styles = {
  success: { icon: 'check-circle', classes: 'border-emerald-200 text-emerald-800' },
  error: { icon: 'warning', classes: 'border-rose-200 text-rose-800' },
  info: { icon: 'info', classes: 'border-gold-200 text-gold-800' },
} as const
</script>

<template>
  <div class="pointer-events-none fixed inset-x-4 bottom-4 z-[70] flex flex-col items-center gap-2 sm:inset-x-auto sm:right-6 sm:items-end" aria-live="polite">
    <TransitionGroup
      enter-active-class="transition duration-300 ease-out"
      enter-from-class="opacity-0 translate-y-3"
      leave-active-class="transition duration-200 ease-in"
      leave-to-class="opacity-0 translate-x-6"
    >
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="pointer-events-auto flex w-full max-w-sm items-start gap-3 rounded-2xl border bg-white px-4 py-3 shadow-lift"
        :class="styles[toast.type].classes"
        role="status"
      >
        <AppIcon :name="styles[toast.type].icon" :size="20" class="mt-0.5" />
        <p class="flex-1 text-sm text-charcoal">{{ toast.message }}</p>
        <button type="button" class="cursor-pointer text-charcoal-soft hover:text-charcoal" aria-label="Dismiss" @click="dismiss(toast.id)">
          <AppIcon name="close" :size="16" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>
