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
    Synced: 'bg-teal/15 text-teal-dark',
    Suspended: 'bg-warning-yellow/15 text-golden-sand-dark',
    Pending: 'bg-warning-yellow/15 text-golden-sand-dark',
    PendingActivation: 'bg-warning-yellow/15 text-golden-sand-dark',
    NotSynced: 'bg-warning-yellow/15 text-golden-sand-dark',
    Disabled: 'bg-pale-olive/30 text-charcoal',
    Expired: 'bg-red-coral/15 text-red-coral',
    Overdue: 'bg-red-coral/15 text-red-coral',
    Cancelled: 'bg-red-coral/15 text-red-coral',
    Failed: 'bg-red-coral/15 text-red-coral',
  }
  return map[props.status] || 'bg-pale-olive/30 text-charcoal'
})

const arabicLabel = computed(() => {
  const labels: Record<string, string> = {
    Active: 'نشط',
    Paid: 'مدفوع',
    Completed: 'مكتمل',
    Synced: 'متزامن',
    Suspended: 'موقوف',
    Pending: 'قيد الانتظار',
    PendingActivation: 'بانتظار التفعيل',
    NotSynced: 'غير متزامن',
    Disabled: 'معطل',
    Expired: 'منتهي',
    Overdue: 'متأخر',
    Cancelled: 'ملغي',
    Failed: 'فشل',
    PartiallyPaid: 'مدفوع جزئياً',
    Unpaid: 'غير مدفوع',
    Draft: 'مسودة',
    Issued: 'صادرة',
  }
  return labels[props.status] || props.status
})
</script>

<template>
  <span :class="[colorClass, 'px-2 py-1 text-xs font-semibold rounded-full']">
    {{ arabicLabel }}
  </span>
</template>
