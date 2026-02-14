<script setup lang="ts">
import { useFormatters } from '@/composables/useFormatters'
import DOMPurify from 'dompurify'

export interface Column {
  label: string
  field: string
  type?: 'text' | 'date' | 'datetime' | 'currency' | 'custom'
  render?: (row: Record<string, unknown>) => string
}

defineProps<{
  data: Record<string, unknown>[]
  columns: Column[]
}>()

const { formatDate, formatDateTime, formatCurrency } = useFormatters()

function getCellValue(row: Record<string, unknown>, col: Column): string {
  if (col.render) {
    return DOMPurify.sanitize(col.render(row))
  }
  const val = row[col.field]
  if (val == null) return '-'
  if (col.type === 'date') return formatDate(String(val))
  if (col.type === 'datetime') return formatDateTime(String(val))
  if (col.type === 'currency') return formatCurrency(Number(val))
  return DOMPurify.sanitize(String(val))
}
</script>

<template>
  <div v-if="!data || data.length === 0" class="text-center py-12 text-gray-500">
    <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
        d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
    </svg>
    <p class="mt-2">لا توجد بيانات متاحة</p>
  </div>

  <div v-else class="overflow-x-auto">
    <table class="min-w-full divide-y divide-gray-200">
      <thead class="bg-gray-50">
        <tr>
          <th
            v-for="col in columns"
            :key="col.field"
            class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
          >
            {{ col.label }}
          </th>
        </tr>
      </thead>
      <tbody class="bg-white divide-y divide-gray-200">
        <tr v-for="(row, idx) in data" :key="idx" class="hover:bg-gray-50">
          <td
            v-for="col in columns"
            :key="col.field"
            class="px-6 py-4 whitespace-nowrap text-sm text-gray-900"
          >
            <slot :name="`cell-${col.field}`" :row="row" :value="getCellValue(row, col)">
              <span v-html="getCellValue(row, col)"></span>
            </slot>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
