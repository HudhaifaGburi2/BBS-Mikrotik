<script setup lang="ts">
import { onMounted } from 'vue'
import { useInvoicesStore } from '@/stores/invoices'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const store = useInvoicesStore()
const { formatCurrency, formatDate } = useFormatters()

onMounted(() => store.fetchAll())
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-6">فواتيري</h1>

    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <table v-if="store.invoices.length" class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">رقم الفاتورة</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المبلغ</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">تاريخ الاستحقاق</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-200">
          <tr v-for="inv in store.invoices" :key="inv.id" class="hover:bg-soft-beige-light">
            <td class="px-6 py-4 text-sm font-medium text-charcoal">{{ inv.invoiceNumber }}</td>
            <td class="px-6 py-4 text-sm text-charcoal font-semibold">{{ formatCurrency(inv.totalAmount) }}</td>
            <td class="px-6 py-4 text-sm text-light-gray">{{ formatDate(inv.dueDate) }}</td>
            <td class="px-6 py-4"><StatusBadge :status="inv.status" /></td>
          </tr>
        </tbody>
      </table>
      <div v-else class="text-center py-12 text-light-gray">لا توجد فواتير</div>
    </div>
  </div>
</template>
