<script setup lang="ts">
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()

const colorMap: Record<string, string> = {
  success: 'bg-teal',
  error: 'bg-red-coral',
  warning: 'bg-warning-yellow',
  info: 'bg-coastal-blue',
}
</script>

<template>
  <div class="fixed top-4 right-4 z-50 max-w-sm space-y-2">
    <TransitionGroup name="toast">
      <div
        v-for="msg in toast.messages"
        :key="msg.id"
        :class="[colorMap[msg.type], 'text-white px-6 py-4 rounded-lg shadow-lg cursor-pointer']"
        @click="toast.remove(msg.id)"
      >
        {{ msg.message }}
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>
