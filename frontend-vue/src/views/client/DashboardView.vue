<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useSubscriptionsStore } from '@/stores/subscriptions'
import { useInvoicesStore } from '@/stores/invoices'
import { useFormatters } from '@/composables/useFormatters'
import { apiGet } from '@/services/http'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import type { Subscriber } from '@/types'

const auth = useAuthStore()
const subscriptionsStore = useSubscriptionsStore()
const invoicesStore = useInvoicesStore()
const { formatCurrency, formatDate } = useFormatters()
const isLoading = ref(false)
const myProfile = ref<Subscriber | null>(null)

onMounted(async () => {
  isLoading.value = true
  try {
    const [, , profileRes] = await Promise.all([
      subscriptionsStore.fetchAll(),
      invoicesStore.fetchAll(),
      apiGet<Subscriber>('/subscribers/me'),
    ])
    if (profileRes.data) {
      myProfile.value = profileRes.data
    }
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-6">مرحباً، {{ auth.fullName }}</h1>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
      <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <h3 class="text-lg font-semibold text-jazan-green mb-4">اشتراكي الحالي</h3>
        <div v-if="subscriptionsStore.subscriptions.length">
          <div v-for="sub in subscriptionsStore.subscriptions.slice(0, 1)" :key="sub.id" class="space-y-2">
            <div class="flex justify-between"><span class="text-light-gray">الباقة:</span><span class="font-semibold text-charcoal">{{ sub.planName }}</span></div>
            <div class="flex justify-between"><span class="text-light-gray">الحالة:</span><StatusBadge :status="sub.status" /></div>
            <div class="flex justify-between"><span class="text-light-gray">تاريخ الانتهاء:</span><span class="text-charcoal">{{ formatDate(sub.endDate) }}</span></div>
            <div class="flex justify-between"><span class="text-light-gray">الأيام المتبقية:</span><span class="font-semibold text-golden-sand-dark">{{ sub.remainingDays }}</span></div>
          </div>
        </div>
        <p v-else class="text-light-gray">لا يوجد اشتراك نشط</p>
      </div>

      <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <h3 class="text-lg font-semibold text-jazan-green mb-4">آخر الفواتير</h3>
        <div v-if="invoicesStore.invoices.length" class="space-y-3">
          <div v-for="inv in invoicesStore.invoices.slice(0, 3)" :key="inv.id" class="flex justify-between items-center py-2 border-b border-soft-beige last:border-0">
            <div>
              <p class="text-sm font-medium text-charcoal">{{ inv.invoiceNumber }}</p>
              <p class="text-xs text-light-gray">{{ formatDate(inv.issueDate) }}</p>
            </div>
            <div class="text-left">
              <p class="text-sm font-semibold text-charcoal">{{ formatCurrency(inv.totalAmount) }}</p>
              <StatusBadge :status="inv.status" />
            </div>
          </div>
        </div>
        <p v-else class="text-light-gray">لا توجد فواتير</p>
      </div>
    </div>

    <!-- Network Info Card -->
    <div v-if="myProfile" class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
      <h3 class="text-lg font-semibold text-coastal-blue mb-4">معلومات الشبكة</h3>
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 text-sm">
        <div class="bg-soft-beige-light rounded-lg p-4">
          <p class="text-light-gray mb-1">اسم مستخدم MikroTik</p>
          <p class="font-semibold text-charcoal font-mono">{{ myProfile.mikroTikUsername || '—' }}</p>
        </div>
        <div class="bg-soft-beige-light rounded-lg p-4">
          <p class="text-light-gray mb-1">عنوان MAC</p>
          <p class="font-semibold text-charcoal font-mono">{{ myProfile.macAddress || '—' }}</p>
        </div>
        <div class="bg-soft-beige-light rounded-lg p-4">
          <p class="text-light-gray mb-1">عنوان IP</p>
          <p class="font-semibold text-charcoal font-mono">{{ myProfile.ipAddress || '—' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>
