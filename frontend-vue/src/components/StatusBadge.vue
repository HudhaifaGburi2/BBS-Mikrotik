<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  status: string
}>()

const colorClass = computed(() => {
  const map: Record<string, string> = {
    Active: 'bg-teal/15 text-teal-dark',
    Paid: 'bg-teal/15 text-teal-dark',
    Completed: 'bg-teal/15 text-teal-dark',
    Suspended: 'bg-warning-yellow/15 text-golden-sand-dark',
    Pending: 'bg-warning-yellow/15 text-golden-sand-dark',
    Expired: 'bg-red-coral/15 text-red-coral',
    Overdue: 'bg-red-coral/15 text-red-coral',
    Cancelled: 'bg-red-coral/15 text-red-coral',
    Failed: 'bg-red-coral/15 text-red-coral',
  }
  return map[props.status] || 'bg-pale-olive/30 text-charcoal'
})
</script>

<template>
  <span :class="[colorClass, 'px-2 py-1 text-xs font-semibold rounded-full']">
    {{ status }}
  </span>
</template>
