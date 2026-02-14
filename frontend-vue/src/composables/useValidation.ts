import { ref, type Ref } from 'vue'
import DOMPurify from 'dompurify'

export interface ValidationRule {
  validate: (value: string) => boolean
  message: string
}

export interface FieldErrors {
  [key: string]: string
}

export function useValidation() {
  const errors: Ref<FieldErrors> = ref({})

  function validateEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
  }

  function validatePhone(phone: string): boolean {
    return /^(\+966|0)?5\d{8}$/.test(phone.replace(/\s+/g, ''))
  }

  function sanitize(input: string): string {
    return DOMPurify.sanitize(input, { ALLOWED_TAGS: [] })
  }

  function required(value: string): boolean {
    return value.trim().length > 0
  }

  function validateField(field: string, value: string, rules: ValidationRule[]): boolean {
    for (const rule of rules) {
      if (!rule.validate(value)) {
        errors.value[field] = rule.message
        return false
      }
    }
    delete errors.value[field]
    return true
  }

  function clearErrors(): void {
    errors.value = {}
  }

  function clearFieldError(field: string): void {
    delete errors.value[field]
  }

  function hasErrors(): boolean {
    return Object.keys(errors.value).length > 0
  }

  return {
    errors,
    validateEmail,
    validatePhone,
    sanitize,
    required,
    validateField,
    clearErrors,
    clearFieldError,
    hasErrors,
  }
}
