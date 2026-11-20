<script setup lang="ts">
import type { CategoryId, Ornament, OrnamentInput } from '~/types'
import { categories } from '~/data/categories'

definePageMeta({ layout: 'admin', middleware: 'admin' })
useHead({ title: 'Manage Ornaments' })

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { items, loading, error, load, create, update, remove } = useOrnaments()
const { search, category, filtered } = useOrnamentFilters(items)
load(true) // admins always work on fresh catalogue data

// ----- Add / edit modal -----
const formOpen = ref(false)
const editing = ref<Ornament | null>(null)
const saving = ref(false)
// Re-mount the form for each open so it starts from fresh values.
const formKey = ref(0)
const formServerErrors = ref<Record<string, string>>({})

function openCreate() {
  formServerErrors.value = {}
  editing.value = null
  formKey.value++
  formOpen.value = true
}

function openEdit(ornament: Ornament) {
  formServerErrors.value = {}
  editing.value = ornament
  formKey.value++
  formOpen.value = true
}

function closeForm() {
  if (saving.value) return
  formOpen.value = false
  if (route.query.new) router.replace({ query: {} })
}

if (route.query.new) openCreate()


async function save(input: OrnamentInput) {
  saving.value = true
  formServerErrors.value = {}
  try {
    if (editing.value) {
      await update(editing.value.id, input)
      toast.success(`“${input.name}” has been updated.`)
    }
    else {
      await create(input)
      toast.success(`“${input.name}” has been added to the catalogue.`)
    }
    saving.value = false
    closeForm()
  }
  catch (error) {
    // Validation (400) and duplicate SKU (409) are shown on the form fields; other errors as a toast.
    if (error instanceof ApiError && error.status === 409) formServerErrors.value = { sku: error.message }
    else if (error instanceof ApiError && Object.keys(error.fieldErrors).length) formServerErrors.value = { ...error.fieldErrors }
    else toast.error(errorMessage(error))
    saving.value = false
  }
}

// ----- Delete -----
const pendingDelete = ref<Ornament | null>(null)
const deleting = ref(false)

async function confirmDelete() {
  if (!pendingDelete.value) return
  deleting.value = true
  try {
    const name = pendingDelete.value.name
    await remove(pendingDelete.value.id)
    toast.success(`“${name}” has been deleted.`)
    pendingDelete.value = null
  }
  catch (error) {
    toast.error(errorMessage(error))
  }
  finally {
    deleting.value = false
  }
}

const categoryOptions: { id: CategoryId | 'all', name: string }[] = [{ id: 'all', name: 'All categories' }, ...categories]
</script>

<template>
  <div class="space-y-6">
    <header class="flex flex-wrap items-end justify-between gap-4">
      <div>
        <p class="eyebrow">Catalogue</p>
        <h1 class="section-title mt-1">Ornament Management</h1>
        <p class="mt-1 text-sm text-charcoal-soft">{{ items.length }} ornaments · changes are stored in this browser only</p>
      </div>
      <button type="button" class="btn btn-primary" @click="openCreate"><AppIcon name="plus" :size="16" /> Add ornament</button>
    </header>

    <div class="card flex flex-col gap-3 p-4 sm:flex-row">
      <div class="flex-1"><SearchBar v-model="search" label="admin-ornaments" placeholder="Search ornaments…" /></div>
      <label class="sr-only" for="admin-category">Category</label>
      <select id="admin-category" v-model="category" class="input rounded-full! py-3 sm:w-56">
        <option v-for="option in categoryOptions" :key="option.id" :value="option.id">{{ option.name }}</option>
      </select>
    </div>

    <LoadingIndicator v-if="loading && !items.length" label="Loading catalogue…" />
    <EmptyState v-else-if="error && !items.length" icon="warning" title="Unable to load the catalogue" :description="error">
      <button type="button" class="btn btn-primary" @click="load(true)">Try again</button>
    </EmptyState>
    <EmptyState v-else-if="!filtered.length" icon="gem" title="No ornaments found" description="Try another search, or add a new ornament to the catalogue.">
      <button type="button" class="btn btn-primary" @click="openCreate">Add ornament</button>
    </EmptyState>

    <template v-else>
      <!-- Desktop table -->
      <div class="card hidden overflow-x-auto md:block">
        <table class="w-full text-left text-sm">
          <thead class="border-b border-gold-100 bg-ivory text-xs uppercase tracking-wider text-charcoal-soft">
            <tr>
              <th class="px-5 py-3 font-medium">Ornament</th>
              <th class="px-5 py-3 font-medium">Category</th>
              <th class="px-5 py-3 font-medium">Price</th>
              <th class="px-5 py-3 font-medium">Availability</th>
              <th class="px-5 py-3 text-right font-medium">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gold-100">
            <tr v-for="item in filtered" :key="item.id" class="transition hover:bg-gold-50/50">
              <td class="px-5 py-3">
                <div class="flex items-center gap-3">
                  <OrnamentImage :ornament="item" class="size-12 shrink-0 rounded-xl" />
                  <div class="min-w-0">
                    <p class="flex items-center gap-1.5 font-medium">
                      {{ item.name }}
                      <AppIcon v-if="item.isFeatured" name="sparkles" :size="14" class="text-gold-500" />
                    </p>
                    <p class="text-xs text-charcoal-soft">{{ item.sku }} · {{ item.metal }}</p>
                  </div>
                </div>
              </td>
              <td class="px-5 py-3">{{ categoryName(item.categoryId) }}</td>
              <td class="whitespace-nowrap px-5 py-3 font-medium">{{ formatPrice(item.price) }}</td>
              <td class="px-5 py-3"><span class="badge" :class="availabilityMeta[item.availability].classes">{{ availabilityMeta[item.availability].label }}</span></td>
              <td class="px-5 py-3">
                <div class="flex justify-end gap-1">
                  <NuxtLink :to="`/ornaments/${item.id}`" class="grid size-9 place-items-center rounded-full text-charcoal-soft transition hover:bg-gold-50 hover:text-gold-700" :aria-label="`View ${item.name}`" title="View in store"><AppIcon name="eye" :size="18" /></NuxtLink>
                  <button type="button" class="grid size-9 cursor-pointer place-items-center rounded-full text-charcoal-soft transition hover:bg-gold-50 hover:text-gold-700" :aria-label="`Edit ${item.name}`" title="Edit" @click="openEdit(item)"><AppIcon name="edit" :size="18" /></button>
                  <button type="button" class="grid size-9 cursor-pointer place-items-center rounded-full text-charcoal-soft transition hover:bg-rose-50 hover:text-rose-600" :aria-label="`Delete ${item.name}`" title="Delete" @click="pendingDelete = item"><AppIcon name="trash" :size="18" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile cards -->
      <div class="grid gap-3 md:hidden">
        <article v-for="item in filtered" :key="item.id" class="card flex gap-4 p-4">
          <OrnamentImage :ornament="item" class="size-20 shrink-0 rounded-xl" />
          <div class="min-w-0 flex-1">
            <p class="truncate font-medium">{{ item.name }}</p>
            <p class="text-xs text-charcoal-soft">{{ categoryName(item.categoryId) }} · {{ formatPrice(item.price) }}</p>
            <span class="badge mt-1" :class="availabilityMeta[item.availability].classes">{{ availabilityMeta[item.availability].label }}</span>
            <div class="mt-2 flex gap-2">
              <button type="button" class="btn btn-outline btn-sm" @click="openEdit(item)"><AppIcon name="edit" :size="14" /> Edit</button>
              <button type="button" class="btn btn-ghost btn-sm text-rose-600!" @click="pendingDelete = item"><AppIcon name="trash" :size="14" /> Delete</button>
            </div>
          </div>
        </article>
      </div>
    </template>

    <AppModal :open="formOpen" :title="editing ? 'Edit ornament' : 'Add ornament'" size="xl" :persistent="saving" @close="closeForm">
      <AdminOrnamentForm
        :key="formKey"
        :initial="editing"
        :submitting="saving"
        :server-errors="formServerErrors"
        @submit="save"
        @cancel="closeForm"
      />
      <template #footer>
        <button type="button" class="btn btn-ghost" :disabled="saving" @click="closeForm">Cancel</button>
        <button type="submit" form="ornament-form" class="btn btn-primary" :disabled="saving">
          <LoadingIndicator v-if="saving" variant="inline" />
          {{ editing ? 'Save changes' : 'Add ornament' }}
        </button>
      </template>
    </AppModal>

    <ConfirmDialog
      :open="pendingDelete !== null"
      title="Delete ornament"
      :message="`“${pendingDelete?.name}” will be permanently removed from the catalogue.`"
      confirm-label="Delete ornament"
      :loading="deleting"
      @confirm="confirmDelete"
      @cancel="pendingDelete = null"
    />
  </div>
</template>
