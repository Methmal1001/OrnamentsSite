<script setup lang="ts">
const props = withDefaults(defineProps<{
  open: boolean
  title: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  /** Prevent closing while an action is in progress. */
  persistent?: boolean
}>(), { size: 'md', persistent: false })

const emit = defineEmits<{ close: [] }>()

const widths = { sm: 'max-w-sm', md: 'max-w-lg', lg: 'max-w-2xl', xl: 'max-w-4xl' }

function requestClose() {
  if (!props.persistent) emit('close')
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && props.open) requestClose()
}

watch(() => props.open, (open) => {
  document.body.style.overflow = open ? 'hidden' : ''
})

onMounted(() => document.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => {
  document.removeEventListener('keydown', onKeydown)
  document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="opacity-0"
      leave-active-class="transition duration-150 ease-in"
      leave-to-class="opacity-0"
    >
      <div v-if="open" class="fixed inset-0 z-[60] flex items-end justify-center bg-charcoal/50 p-0 backdrop-blur-sm sm:items-center sm:p-4" @click.self="requestClose">
        <div
          class="flex max-h-[92vh] w-full animate-fade-up flex-col overflow-hidden rounded-t-3xl bg-white shadow-lift sm:rounded-3xl"
          :class="widths[size]"
          role="dialog"
          aria-modal="true"
          :aria-label="title"
        >
          <header class="flex items-center justify-between gap-4 border-b border-gold-100 px-6 py-4">
            <h2 class="font-serif text-2xl font-semibold">{{ title }}</h2>
            <button type="button" class="grid size-9 cursor-pointer place-items-center rounded-full text-charcoal-soft transition hover:bg-gold-50" aria-label="Close" :disabled="persistent" @click="requestClose">
              <AppIcon name="close" />
            </button>
          </header>
          <div class="overflow-y-auto px-6 py-5">
            <slot />
          </div>
          <footer v-if="$slots.footer" class="flex flex-col-reverse gap-2 border-t border-gold-100 bg-ivory px-6 py-4 sm:flex-row sm:justify-end">
            <slot name="footer" />
          </footer>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
