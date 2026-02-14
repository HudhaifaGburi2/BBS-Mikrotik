<script setup lang="ts">
defineProps<{ visible: boolean }>()
</script>

<template>
  <Teleport to="body">
    <div v-if="visible" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-8">
        <div class="animate-spin rounded-full h-16 w-16 border-b-4 border-blue-600"></div>
      </div>
    </div>
  </Teleport>
</template>
