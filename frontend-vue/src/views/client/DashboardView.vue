<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
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
const { formatCurrency, formatDate, formatBytes } = useFormatters()
const isLoading = ref(false)
const myProfile = ref<Subscriber | null>(null)

// Quota calculations
const currentSub = computed(() => subscriptionsStore.subscriptions[0] || null)

function getDataUsagePercent(usedBytes: number, limitGB: number): number {
  if (!limitGB || limitGB <= 0) return 0
  const limitBytes = limitGB * 1024 * 1024 * 1024
  return Math.min(100, Math.round((usedBytes / limitBytes) * 100))
}

function getRemainingBytes(usedBytes: number, limitGB: number): number {
  if (!limitGB || limitGB <= 0) return 0
  const limitBytes = limitGB * 1024 * 1024 * 1024
  return Math.max(0, limitBytes - usedBytes)
}

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

    <!-- Pending Activation Banner -->
    <div v-if="currentSub?.status === 'PendingActivation'" class="bg-golden-sand/10 border border-golden-sand rounded-xl p-4 mb-6">
      <div class="flex items-center gap-3">
        <div class="w-12 h-12 rounded-full bg-golden-sand/20 flex items-center justify-center">
          <svg class="w-6 h-6 text-golden-sand-dark" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <div class="flex-1">
          <h3 class="text-lg font-bold text-golden-sand-dark">اشتراكك في انتظار التفعيل</h3>
          <p class="text-sm text-charcoal">يرجى إتمام الدفع لتفعيل الاشتراك والوصول للإنترنت</p>
        </div>
        <router-link 
          to="/client/payment" 
          class="px-4 py-2 bg-golden-sand text-white rounded-lg hover:bg-golden-sand-dark transition-colors flex items-center gap-2"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          الدفع الآن
        </router-link>
      </div>
    </div>

    <!-- Suspended/Quota Exceeded Alert Banner -->
    <div v-if="currentSub?.dataLimitExceeded" class="bg-red-100 border border-red-coral rounded-xl p-4 mb-6">
      <div class="flex items-center gap-3">
        <div class="w-12 h-12 rounded-full bg-red-coral/20 flex items-center justify-center">
          <svg class="w-6 h-6 text-red-coral" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <div>
          <h3 class="text-lg font-bold text-red-coral">تم تجاوز حد البيانات</h3>
          <p class="text-sm text-red-coral/80">تم إيقاف الاتصال بالإنترنت. يرجى تجديد الاشتراك أو الترقية لباقة أكبر.</p>
        </div>
      </div>
    </div>

    <!-- Data Quota Card -->
    <div v-if="currentSub && currentSub.dataLimitGB > 0" class="bg-white rounded-xl shadow-md p-6 border border-soft-beige mb-6">
      <h3 class="text-lg font-semibold text-coastal-blue mb-4">حصة البيانات</h3>
      <div class="grid grid-cols-3 gap-4 text-center mb-4">
        <div>
          <p class="text-xs text-light-gray mb-1">المستخدم</p>
          <p class="text-xl font-bold text-golden-sand-dark">{{ formatBytes(currentSub.dataUsedBytes || 0) }}</p>
        </div>
        <div>
          <p class="text-xs text-light-gray mb-1">الحصة</p>
          <p class="text-xl font-bold text-charcoal">{{ currentSub.dataLimitGB }} GB</p>
        </div>
        <div>
          <p class="text-xs text-light-gray mb-1">المتبقي</p>
          <p class="text-xl font-bold" :class="getRemainingBytes(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB) > 0 ? 'text-jazan-green' : 'text-red-coral'">
            {{ formatBytes(getRemainingBytes(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB)) }}
          </p>
        </div>
      </div>
      <!-- Progress Bar -->
      <div class="w-full bg-gray-200 rounded-full h-4">
        <div 
          class="h-4 rounded-full transition-all duration-300"
          :class="getDataUsagePercent(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB) >= 90 ? 'bg-red-coral' : getDataUsagePercent(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB) >= 70 ? 'bg-warning-yellow' : 'bg-jazan-green'"
          :style="{ width: getDataUsagePercent(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB) + '%' }"
        ></div>
      </div>
      <p class="text-center text-sm mt-2" :class="getDataUsagePercent(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB) >= 90 ? 'text-red-coral font-semibold' : 'text-light-gray'">
        {{ getDataUsagePercent(currentSub.dataUsedBytes || 0, currentSub.dataLimitGB) }}% مستهلك
      </p>
    </div>

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
