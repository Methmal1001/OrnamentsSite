import type { ValidationRule } from '~/utils/validators'
import { ApiError } from './useApi'

/**
 * Field-level validation for a reactive form object.
 *
 * const form = reactive({ email: '' })
 * const { errors, validate, validateField } = useFormValidation(form, { email: [rules.required('Email'), rules.email()] })
 */
export function useFormValidation<T extends object>(form: T, schema: Partial<Record<keyof T & string, ValidationRule[]>>) {
  const errors = reactive<Record<string, string | undefined>>({})

  function validateField(field: keyof T & string): boolean {
    const value = (form as Record<string, unknown>)[field]
    for (const rule of schema[field] ?? []) {
      const result = rule(value)
      if (result !== true) {
        errors[field] = result
        return false
      }
    }
    errors[field] = undefined
    return true
  }

  function validate(): boolean {
    return (Object.keys(schema) as Array<keyof T & string>)
      .map(validateField)
      .every(Boolean)
  }

  // Once a field shows an error, re-check it live so the message clears as soon as it is fixed.
  watch(() => ({ ...form }), () => {
    (Object.keys(errors) as Array<keyof T & string>)
      .filter(field => errors[field])
      .forEach(validateField)
  }, { deep: true })

  function clearErrors() {
    Object.keys(errors).forEach((key) => { errors[key] = undefined })
  }

  /**
   * Shows backend validation messages next to the matching fields.
   * Returns true when at least one field error was applied.
   */
  function applyServerErrors(error: unknown): boolean {
    if (!(error instanceof ApiError)) return false
    let applied = false
    for (const [field, message] of Object.entries(error.fieldErrors)) {
      if (field in form) {
        errors[field] = message
        applied = true
      }
    }
    return applied
  }

  return { errors, validate, validateField, clearErrors, applyServerErrors }
}

/** Extracts a user-facing message from any thrown value. */
export function errorMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  return error instanceof Error && error.message ? error.message : fallback
}
