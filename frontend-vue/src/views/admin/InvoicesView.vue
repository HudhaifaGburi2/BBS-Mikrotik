<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useInvoicesStore } from '@/stores/invoices'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const store = useInvoicesStore()
const router = useRouter()
const { formatCurrency, formatDate } = useFormatters()

const filter = ref<'all' | 'overdue'>('all')

async function loadInvoices() {
  if (filter.value === 'overdue') {
    await store.fetchOverdue()
  } else {
    await store.fetchAll()
  }
}

function setFilter(f: 'all' | 'overdue') {
  filter.value = f
  loadInvoices()
}

onMounted(() => loadInvoices())
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">الفواتير</h1>
      <div class="flex gap-2">
        <button
          :class="[filter === 'all' ? 'bg-coastal-blue text-white' : 'bg-pale-olive/30 text-charcoal', 'px-4 py-2 rounded-lg text-sm transition-colors']"
          @click="setFilter('all')"
        >الكل</button>
        <button
          :class="[filter === 'overdue' ? 'bg-red-coral text-white' : 'bg-pale-olive/30 text-charcoal', 'px-4 py-2 rounded-lg text-sm transition-colors']"
          @click="setFilter('overdue')"
        >متأخرة</button>
      </div>
    </div>

    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <table v-if="store.invoices.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">رقم الفاتورة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المشترك</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المبلغ</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">تاريخ الإصدار</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">تاريخ الاستحقاق</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="inv in store.invoices" :key="inv.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-charcoal">{{ inv.invoiceNumber }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ inv.subscriberName }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-charcoal font-semibold">{{ formatCurrency(inv.totalAmount) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ formatDate(inv.issueDate) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ formatDate(inv.dueDate) }}</td>
              <td class="px-6 py-4 whitespace-nowrap"><StatusBadge :status="inv.status" /></td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <button class="text-coastal-blue hover:text-coastal-blue-dark font-medium" @click="router.push(`/admin/invoices/${inv.id}`)">عرض</button>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="text-center py-12 text-light-gray">لا توجد فواتير</div>
      </div>
    </div>
  </div>
</template>
