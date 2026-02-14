<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useInvoicesStore } from '@/stores/invoices'
import { usePaymentsStore } from '@/stores/payments'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const route = useRoute()
const invoiceStore = useInvoicesStore()
const paymentsStore = usePaymentsStore()
const { formatCurrency, formatDate } = useFormatters()

onMounted(async () => {
  const id = route.params.id as string
  await invoiceStore.fetchById(id)
  await paymentsStore.fetchByInvoice(id)
})
</script>

<template>
  <AppLoader :visible="invoiceStore.isLoading" />
  <div v-if="invoiceStore.currentInvoice" class="max-w-4xl mx-auto">
    <div class="bg-white rounded-xl shadow-md p-8 border border-soft-beige">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-bold text-coastal-blue">فاتورة #{{ invoiceStore.currentInvoice.invoiceNumber }}</h1>
          <p class="text-light-gray mt-1">{{ invoiceStore.currentInvoice.subscriberName }}</p>
        </div>
        <StatusBadge :status="invoiceStore.currentInvoice.status" />
      </div>

      <div class="grid grid-cols-2 md:grid-cols-4 gap-6 mb-8">
        <div>
          <p class="text-sm text-light-gray">تاريخ الإصدار</p>
          <p class="font-semibold">{{ formatDate(invoiceStore.currentInvoice.issueDate) }}</p>
        </div>
        <div>
          <p class="text-sm text-light-gray">تاريخ الاستحقاق</p>
          <p class="font-semibold">{{ formatDate(invoiceStore.currentInvoice.dueDate) }}</p>
        </div>
        <div>
          <p class="text-sm text-light-gray">المبلغ الإجمالي</p>
          <p class="font-semibold">{{ formatCurrency(invoiceStore.currentInvoice.totalAmount) }}</p>
        </div>
        <div>
          <p class="text-sm text-light-gray">المبلغ المتبقي</p>
          <p class="font-semibold text-red-coral">{{ formatCurrency(invoiceStore.currentInvoice.remainingAmount) }}</p>
        </div>
      </div>

      <div class="border-t border-gray-200 pt-6">
        <h2 class="text-lg font-semibold text-jazan-green mb-4">تفاصيل المبالغ</h2>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between"><span class="text-light-gray">المبلغ الفرعي:</span><span>{{ formatCurrency(invoiceStore.currentInvoice.subtotal) }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">الضريبة:</span><span>{{ formatCurrency(invoiceStore.currentInvoice.taxAmount) }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">الخصم:</span><span>-{{ formatCurrency(invoiceStore.currentInvoice.discountAmount) }}</span></div>
          <div class="flex justify-between font-bold border-t pt-2"><span>الإجمالي:</span><span>{{ formatCurrency(invoiceStore.currentInvoice.totalAmount) }}</span></div>
          <div class="flex justify-between text-teal"><span>المدفوع:</span><span>{{ formatCurrency(invoiceStore.currentInvoice.paidAmount) }}</span></div>
        </div>
      </div>

      <div class="border-t border-gray-200 pt-6 mt-6">
        <h2 class="text-lg font-semibold text-jazan-green mb-4">المدفوعات</h2>
        <table v-if="paymentsStore.payments.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-2 text-right text-xs font-medium text-coastal-blue">المرجع</th>
              <th class="px-4 py-2 text-right text-xs font-medium text-coastal-blue">المبلغ</th>
              <th class="px-4 py-2 text-right text-xs font-medium text-coastal-blue">الطريقة</th>
              <th class="px-4 py-2 text-right text-xs font-medium text-coastal-blue">التاريخ</th>
              <th class="px-4 py-2 text-right text-xs font-medium text-coastal-blue">الحالة</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            <tr v-for="p in paymentsStore.payments" :key="p.id">
              <td class="px-4 py-2 text-sm">{{ p.paymentReference }}</td>
              <td class="px-4 py-2 text-sm">{{ formatCurrency(p.amount) }}</td>
              <td class="px-4 py-2 text-sm">{{ p.method }}</td>
              <td class="px-4 py-2 text-sm">{{ formatDate(p.paymentDate) }}</td>
              <td class="px-4 py-2"><StatusBadge :status="p.status" /></td>
            </tr>
          </tbody>
        </table>
        <p v-else class="text-light-gray text-center py-4">لا توجد مدفوعات</p>
      </div>
    </div>

    <div class="mt-4">
      <RouterLink to="/admin/invoices" class="text-coastal-blue hover:text-coastal-blue-dark text-sm font-medium">&larr; العودة للفواتير</RouterLink>
    </div>
  </div>
</template>
