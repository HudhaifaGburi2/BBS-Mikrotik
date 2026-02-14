<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  currentPage: number
  totalPages: number
}>()

const emit = defineEmits<{
  'page-change': [page: number]
}>()

const visiblePages = computed(() => {
  const maxVisible = 5
  let start = Math.max(1, props.currentPage - Math.floor(maxVisible / 2))
  const end = Math.min(props.totalPages, start + maxVisible - 1)
  if (end - start < maxVisible - 1) {
    start = Math.max(1, end - maxVisible + 1)
  }
  const pages: number[] = []
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

function goTo(page: number) {
  if (page >= 1 && page <= props.totalPages && page !== props.currentPage) {
    emit('page-change', page)
  }
}
</script>

<template>
  <div v-if="totalPages > 1" class="flex items-center justify-between border-t border-soft-beige bg-white px-4 py-3 sm:px-6">
    <div class="flex flex-1 justify-between sm:hidden">
      <button
        :disabled="currentPage === 1"
        class="relative inline-flex items-center rounded-md border border-sandy-brown/30 bg-white px-4 py-2 text-sm font-medium text-charcoal hover:bg-soft-beige-light disabled:opacity-50 disabled:cursor-not-allowed"
        @click="goTo(currentPage - 1)"
      >
        السابق
      </button>
      <button
        :disabled="currentPage === totalPages"
        class="relative ml-3 inline-flex items-center rounded-md border border-sandy-brown/30 bg-white px-4 py-2 text-sm font-medium text-charcoal hover:bg-soft-beige-light disabled:opacity-50 disabled:cursor-not-allowed"
        @click="goTo(currentPage + 1)"
      >
        التالي
      </button>
    </div>
    <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
      <p class="text-sm text-charcoal">
        صفحة <span class="font-medium">{{ currentPage }}</span> من <span class="font-medium">{{ totalPages }}</span>
      </p>
      <nav class="isolate inline-flex -space-x-px rounded-md shadow-sm">
        <button
          :disabled="currentPage === 1"
          class="relative inline-flex items-center rounded-l-md px-2 py-2 text-light-gray ring-1 ring-inset ring-sandy-brown/30 hover:bg-soft-beige-light disabled:opacity-50 disabled:cursor-not-allowed"
          @click="goTo(currentPage - 1)"
        >
          <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clip-rule="evenodd" />
          </svg>
        </button>
        <button
          v-for="page in visiblePages"
          :key="page"
          :class="[
            'relative inline-flex items-center px-4 py-2 text-sm font-semibold',
            page === currentPage
              ? 'z-10 bg-jazan-green text-white'
              : 'text-charcoal ring-1 ring-inset ring-sandy-brown/30 hover:bg-soft-beige-light',
          ]"
          @click="goTo(page)"
        >
          {{ page }}
        </button>
        <button
          :disabled="currentPage === totalPages"
          class="relative inline-flex items-center rounded-r-md px-2 py-2 text-light-gray ring-1 ring-inset ring-sandy-brown/30 hover:bg-soft-beige-light disabled:opacity-50 disabled:cursor-not-allowed"
          @click="goTo(currentPage + 1)"
        >
          <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd" />
          </svg>
        </button>
      </nav>
    </div>
  </div>
</template>
