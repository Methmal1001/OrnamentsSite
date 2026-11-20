<script setup lang="ts">
/**
 * Labelled input with error message. `type="password"` adds a show/hide toggle,
 * `multiline` renders a textarea, `type="number"` emits numbers.
 */
const props = withDefaults(defineProps<{
  label: string
  type?: 'text' | 'email' | 'tel' | 'password' | 'number' | 'url'
  placeholder?: string
  error?: string
  hint?: string
  autocomplete?: string
  multiline?: boolean
  rows?: number
  required?: boolean
  disabled?: boolean
  step?: string
}>(), {
  type: 'text', placeholder: '', error: undefined, hint: '', autocomplete: 'off',
  multiline: false, rows: 3, required: false, disabled: false, step: undefined,
})

const model = defineModel<string | number>({ default: '' })
const emit = defineEmits<{ blur: [] }>()

const id = useId()
const revealed = ref(false)
const inputType = computed(() => (props.type === 'password' && revealed.value ? 'text' : props.type))
const describedBy = computed(() => (props.error ? `${id}-error` : props.hint ? `${id}-hint` : undefined))

function onInput(event: Event) {
  const target = event.target as HTMLInputElement | HTMLTextAreaElement
  model.value = props.type === 'number' && target instanceof HTMLInputElement ? target.valueAsNumber : target.value
}

const displayValue = computed(() => (typeof model.value === 'number' && Number.isNaN(model.value) ? '' : model.value))
</script>

<template>
  <div>
    <label :for="id" class="label">
      {{ label }}<span v-if="required" class="text-gold-600"> *</span>
    </label>
    <div class="relative">
      <textarea
        v-if="multiline"
        :id="id"
        :value="displayValue"
        :rows="rows"
        :placeholder="placeholder"
        :disabled="disabled"
        :aria-invalid="!!error"
        :aria-describedby="describedBy"
        class="input resize-y"
        :class="{ 'input-error': error }"
        @input="onInput"
        @blur="emit('blur')"
      />
      <input
        v-else
        :id="id"
        :value="displayValue"
        :type="inputType"
        :placeholder="placeholder"
        :autocomplete="autocomplete"
        :disabled="disabled"
        :step="step"
        :aria-invalid="!!error"
        :aria-describedby="describedBy"
        class="input disabled:bg-beige/60 disabled:text-charcoal-soft"
        :class="{ 'input-error': error, 'pr-12': type === 'password' }"
        @input="onInput"
        @blur="emit('blur')"
      >
      <button
        v-if="type === 'password'"
        type="button"
        class="absolute right-2 top-1/2 grid size-8 -translate-y-1/2 cursor-pointer place-items-center rounded-full text-charcoal-soft transition hover:bg-gold-50 hover:text-gold-700"
        :aria-label="revealed ? 'Hide password' : 'Show password'"
        :aria-pressed="revealed"
        @click="revealed = !revealed"
      >
        <AppIcon :name="revealed ? 'eye-off' : 'eye'" :size="18" />
      </button>
    </div>
    <p v-if="error" :id="`${id}-error`" class="mt-1.5 text-xs text-rose-600">{{ error }}</p>
    <p v-else-if="hint" :id="`${id}-hint`" class="mt-1.5 text-xs text-charcoal-soft">{{ hint }}</p>
  </div>
</template>
