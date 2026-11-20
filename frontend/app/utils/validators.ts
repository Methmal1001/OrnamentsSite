/**
 * Small, composable validation rules. Each rule returns `true` when valid
 * or an error message string when invalid.
 */
export type ValidationRule = (value: unknown) => true | string

const asText = (value: unknown) => (value ?? '').toString().trim()

export const rules = {
  required: (label = 'This field'): ValidationRule =>
    value => asText(value).length > 0 || `${label} is required`,

  email: (): ValidationRule =>
    value => /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(asText(value)) || 'Enter a valid email address',

  /** Sri Lankan numbers: 0771234567, +94771234567, 011 234 5678 … */
  phone: (): ValidationRule =>
    value => /^(?:\+94|0)\d{9}$/.test(asText(value).replace(/[\s-]/g, '')) || 'Enter a valid Sri Lankan phone number',

  minLength: (min: number, label = 'This field'): ValidationRule =>
    value => asText(value).length >= min || `${label} must be at least ${min} characters`,

  maxLength: (max: number, label = 'This field'): ValidationRule =>
    value => asText(value).length <= max || `${label} must be ${max} characters or fewer`,

  strongPassword: (): ValidationRule =>
    (value) => {
      const text = (value ?? '').toString()
      if (text.length < 8) return 'Password must be at least 8 characters'
      if (!/[A-Z]/.test(text) || !/[a-z]/.test(text)) return 'Use both upper- and lower-case letters'
      if (!/\d/.test(text)) return 'Include at least one number'
      return true
    },

  sameAs: (other: () => unknown, message = 'Values do not match'): ValidationRule =>
    value => value === other() || message,

  positiveNumber: (label = 'Value'): ValidationRule =>
    value => (typeof value === 'number' && Number.isFinite(value) && value > 0) || `${label} must be greater than zero`,
}
