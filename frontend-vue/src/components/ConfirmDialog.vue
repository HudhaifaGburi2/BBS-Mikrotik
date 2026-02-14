<script setup lang="ts">
import AppModal from './AppModal.vue'

defineProps<{
  visible: boolean
  title: string
  message: string
  confirmText?: string
  cancelText?: string
}>()

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<template>
  <AppModal :visible="visible" :title="title" @close="emit('cancel')">
    <div class="text-center">
      <p class="text-charcoal mb-6">{{ message }}</p>
      <div class="flex justify-center gap-4">
        <button
          class="px-4 py-2 bg-pale-olive/30 text-charcoal rounded-lg hover:bg-pale-olive/50 transition-colors"
          @click="emit('cancel')"
        >
          {{ cancelText || 'إلغاء' }}
        </button>
        <button
          class="px-4 py-2 bg-red-coral text-white rounded-lg hover:bg-red-coral-dark transition-colors"
          @click="emit('confirm')"
        >
          {{ confirmText || 'تأكيد' }}
        </button>
      </div>
    </div>
  </AppModal>
</template>
