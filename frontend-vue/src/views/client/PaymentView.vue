<script setup lang="ts">
import { onMounted } from 'vue'
import { usePaymentsStore } from '@/stores/payments'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const store = usePaymentsStore()
const { formatCurrency, formatDate } = useFormatters()

onMounted(() => store.fetchAll())
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-6">مدفوعاتي</h1>

    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <table v-if="store.payments.length" class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المرجع</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المبلغ</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الطريقة</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">التاريخ</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-200">
          <tr v-for="p in store.payments" :key="p.id" class="hover:bg-soft-beige-light">
            <td class="px-6 py-4 text-sm font-medium text-charcoal">{{ p.paymentReference }}</td>
            <td class="px-6 py-4 text-sm text-charcoal font-semibold">{{ formatCurrency(p.amount) }}</td>
            <td class="px-6 py-4 text-sm text-light-gray">{{ p.method }}</td>
            <td class="px-6 py-4 text-sm text-light-gray">{{ formatDate(p.paymentDate) }}</td>
            <td class="px-6 py-4"><StatusBadge :status="p.status" /></td>
          </tr>
        </tbody>
      </table>
      <div v-else class="text-center py-12 text-light-gray">لا توجد مدفوعات</div>
    </div>
  </div>
</template>
