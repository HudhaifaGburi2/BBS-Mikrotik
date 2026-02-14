<script setup lang="ts">
defineProps<{
  title: string
  visible: boolean
}>()

const emit = defineEmits<{
  close: []
}>()
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="visible" class="fixed inset-0 z-50 overflow-y-auto">
        <div class="flex items-center justify-center min-h-screen px-4">
          <div class="fixed inset-0 bg-black/50 transition-opacity" @click="emit('close')"></div>
          <div class="relative bg-white rounded-2xl shadow-xl max-w-2xl w-full p-6 border border-soft-beige">
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-xl font-semibold text-coastal-blue">{{ title }}</h3>
              <button class="text-light-gray hover:text-charcoal" @click="emit('close')">
                <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <div>
              <slot />
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
