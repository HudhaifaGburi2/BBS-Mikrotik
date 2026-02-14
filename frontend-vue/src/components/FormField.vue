<script setup lang="ts">
defineProps<{
  label: string
  error?: string
  required?: boolean
}>()
</script>

<template>
  <div>
    <label class="block text-sm font-medium text-charcoal mb-2">
      {{ label }}
      <span v-if="required" class="text-red-coral">*</span>
    </label>
    <slot />
    <p v-if="error" class="text-red-coral text-sm mt-1">{{ error }}</p>
  </div>
</template>
