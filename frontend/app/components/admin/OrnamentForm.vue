<script setup lang="ts">
import type { AvailabilityStatus, Ornament, OrnamentInput } from '~/types'
import { categories } from '~/data/categories'

const props = withDefaults(defineProps<{
  initial?: Ornament | null
  submitting?: boolean
  /** Field errors returned by the API (400 validation or 409 duplicate SKU). */
  serverErrors?: Record<string, string>
}>(), { initial: null, submitting: false, serverErrors: () => ({}) })

const emit = defineEmits<{ submit: [input: OrnamentInput], cancel: [] }>()

const metals = ['22K Yellow Gold', '18K Yellow Gold', '18K Rose Gold', '18K White Gold', 'Platinum']
const gemstones = ['Blue Sapphire', 'Ruby', 'Emerald', 'Diamond', 'Pearl', 'Moonstone', 'Padparadscha', 'Yellow Sapphire', 'Cat\'s Eye']
const availabilityOptions = Object.entries(availabilityMeta) as [AvailabilityStatus, { label: string }][]

function emptyInput(): OrnamentInput {
  return {
    sku: '', name: '', categoryId: 'gold-rings', description: '', price: Number.NaN,
    availability: 'in-stock', metal: '22K Yellow Gold', weightGrams: Number.NaN,
    gemstone: null, imageUrl: null, isFeatured: false,
  }
}

function toInput(ornament: Ornament | null): OrnamentInput {
  if (!ornament) return emptyInput()
  // Server-managed fields are not sent back: stock is derived from the chosen availability.
  const { id: _id, createdAt: _c, updatedAt: _u, stockQuantity: _s, isAvailable: _a, ...input } = ornament
  return { ...input }
}

const values = reactive<OrnamentInput>(toInput(props.initial))
// Text fields for nullable values keep the inputs simple.
const gemstoneText = ref(props.initial?.gemstone ?? '')
const imageUrlText = ref(props.initial?.imageUrl ?? '')

const { errors, validate, validateField } = useFormValidation(values, {
  name: [rules.required('Name'), rules.minLength(3, 'Name')],
  sku: [rules.required('SKU')],
  description: [rules.required('Description'), rules.minLength(20, 'Description')],
  price: [rules.positiveNumber('Price')],
  weightGrams: [rules.positiveNumber('Weight')],
})

const serverImageError = ref<string>()
watch(imageUrlText, () => { serverImageError.value = undefined })

const imageError = computed(() =>
  imageUrlText.value && !/^(https?:\/\/|\/)/.test(imageUrlText.value.trim())
    ? 'Use an absolute URL (https://…) or a path starting with /'
    : serverImageError.value,
)

watch(() => props.serverErrors, (fieldErrors) => {
  for (const [field, message] of Object.entries(fieldErrors)) {
    if (field === 'imageUrl') serverImageError.value = message
    else if (field in values) errors[field] = message
  }
})

const preview = computed(() => ({
  name: values.name || 'New ornament',
  categoryId: values.categoryId,
  metal: values.metal,
  gemstone: gemstoneText.value || null,
  imageUrl: imageError.value ? null : imageUrlText.value.trim() || null,
}))

const pricePreview = computed(() => (Number.isFinite(values.price) ? formatPrice(values.price) : 'Rs. —'))

const isFeatured = computed({
  get: () => values.isFeatured,
  set: (value: boolean) => { values.isFeatured = value },
})

function onSubmit() {
  const valid = validate()
  if (!valid || imageError.value) return
  emit('submit', {
    ...values,
    name: values.name.trim(),
    sku: values.sku.trim().toUpperCase(),
    description: values.description.trim(),
    gemstone: gemstoneText.value || null,
    imageUrl: imageUrlText.value.trim() || null,
  })
}
</script>

<template>
  <form id="ornament-form" class="grid gap-6 lg:grid-cols-[1fr_220px]" novalidate @submit.prevent="onSubmit">
    <div class="space-y-5">
      <div class="grid gap-5 sm:grid-cols-2">
        <FormInput v-model="values.name" label="Ornament name" required :error="errors.name" @blur="validateField('name')" />
        <FormInput v-model="values.sku" label="SKU" placeholder="LO-RG-005" required :error="errors.sku" @blur="validateField('sku')" />
      </div>

      <div class="grid gap-5 sm:grid-cols-2">
        <div>
          <label class="label" for="ornament-category">Category <span class="text-gold-600">*</span></label>
          <select id="ornament-category" v-model="values.categoryId" class="input">
            <option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option>
          </select>
        </div>
        <div>
          <label class="label" for="ornament-availability">Availability <span class="text-gold-600">*</span></label>
          <select id="ornament-availability" v-model="values.availability" class="input">
            <option v-for="[value, meta] in availabilityOptions" :key="value" :value="value">{{ meta.label }}</option>
          </select>
        </div>
      </div>

      <div class="grid gap-5 sm:grid-cols-2">
        <FormInput v-model="values.price" label="Price (Rs.)" type="number" step="500" required :error="errors.price" @blur="validateField('price')" />
        <FormInput v-model="values.weightGrams" label="Weight (grams)" type="number" step="0.1" required :error="errors.weightGrams" @blur="validateField('weightGrams')" />
      </div>

      <div class="grid gap-5 sm:grid-cols-2">
        <div>
          <label class="label" for="ornament-metal">Metal</label>
          <select id="ornament-metal" v-model="values.metal" class="input">
            <option v-for="metal in metals" :key="metal" :value="metal">{{ metal }}</option>
          </select>
        </div>
        <div>
          <label class="label" for="ornament-gem">Gemstone</label>
          <select id="ornament-gem" v-model="gemstoneText" class="input">
            <option value="">None</option>
            <option v-for="gem in gemstones" :key="gem" :value="gem">{{ gem }}</option>
          </select>
        </div>
      </div>

      <FormInput v-model="values.description" label="Description" multiline :rows="4" required :error="errors.description" @blur="validateField('description')" />
      <FormInput v-model="imageUrlText" label="Image URL (optional)" type="url" placeholder="https://example.com/ring.jpg" :error="imageError" hint="Leave empty to use the built-in illustration." />

      <label class="flex cursor-pointer items-center gap-3 text-sm">
        <input v-model="isFeatured" type="checkbox" class="size-4 rounded accent-gold-600">
        Show as a featured ornament on the home page
      </label>
    </div>

    <aside class="order-first lg:order-none">
      <p class="label">Preview</p>
      <div class="card overflow-hidden">
        <OrnamentImage :ornament="preview" class="aspect-square" />
        <div class="p-3">
          <p class="truncate font-serif text-lg font-semibold">{{ preview.name }}</p>
          <p class="text-sm text-gold-700">{{ pricePreview }}</p>
        </div>
      </div>
    </aside>
  </form>
</template>
