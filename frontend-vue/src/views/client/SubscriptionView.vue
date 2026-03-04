<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSubscriptionsStore } from '@/stores/subscriptions'
import { useFormatters } from '@/composables/useFormatters'
import { apiGet } from '@/services/http'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import type { Subscriber } from '@/types'

const store = useSubscriptionsStore()
const { formatDate, formatBytes } = useFormatters()
const myProfile = ref<Subscriber | null>(null)

function getDataUsagePercent(usedBytes: number, limitGB: number): number {
  if (!limitGB || limitGB <= 0) return 0
  const limitBytes = limitGB * 1024 * 1024 * 1024
  return Math.min(100, Math.round((usedBytes / limitBytes) * 100))
}

function getRemainingGB(usedBytes: number, limitGB: number): string {
  if (!limitGB || limitGB <= 0) return 'غير محدود'
  const usedGB = usedBytes / (1024 * 1024 * 1024)
  const remaining = Math.max(0, limitGB - usedGB)
  return remaining.toFixed(2) + ' GB'
}

onMounted(async () => {
  await store.fetchAll()
  try {
    const res = await apiGet<Subscriber>('/subscribers/me')
    if (res.data) myProfile.value = res.data
  } catch { /* profile may not be available */ }
})
</script>

<template>
  <AppLoader :visible="store.isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-6">اشتراكي</h1>

    <div v-if="store.subscriptions.length" class="space-y-6">
      <div v-for="sub in store.subscriptions" :key="sub.id" class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <!-- Pending Activation Banner -->
        <div v-if="sub.status === 'PendingActivation'" class="bg-golden-sand/10 border border-golden-sand rounded-lg p-4 mb-4">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-full bg-golden-sand/20 flex items-center justify-center">
              <svg class="w-5 h-5 text-golden-sand-dark" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <h4 class="font-semibold text-golden-sand-dark">في انتظار التفعيل</h4>
              <p class="text-sm text-charcoal">يرجى إتمام الدفع لتفعيل الاشتراك والوصول للإنترنت</p>
            </div>
          </div>
          <router-link 
            to="/client/payment" 
            class="mt-3 inline-flex items-center gap-2 px-4 py-2 bg-golden-sand text-white rounded-lg hover:bg-golden-sand-dark transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            الدفع الآن
          </router-link>
        </div>

        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold text-coastal-blue">{{ sub.planName }}</h3>
          <StatusBadge :status="sub.status" />
        </div>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
          <div><p class="text-light-gray">تاريخ البداية</p><p class="font-semibold text-charcoal">{{ formatDate(sub.startDate) }}</p></div>
          <div><p class="text-light-gray">تاريخ الانتهاء</p><p class="font-semibold text-charcoal">{{ formatDate(sub.endDate) }}</p></div>
          <div><p class="text-light-gray">الأيام المتبقية</p><p class="font-semibold text-golden-sand-dark">{{ sub.remainingDays }}</p></div>
          <div v-if="sub.activatedAt"><p class="text-light-gray">تاريخ التفعيل</p><p class="font-semibold text-charcoal">{{ formatDate(sub.activatedAt) }}</p></div>
        </div>

        <!-- Data Usage Section -->
        <div class="mt-6 pt-4 border-t border-soft-beige">
          <h4 class="text-sm font-semibold text-coastal-blue mb-3">استهلاك البيانات</h4>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm mb-3">
            <div>
              <p class="text-light-gray">المستهلك</p>
              <p class="font-semibold text-charcoal">{{ formatBytes(sub.dataUsedBytes || 0) }}</p>
            </div>
            <div>
              <p class="text-light-gray">الحد الأقصى</p>
              <p class="font-semibold text-charcoal">{{ sub.dataLimitGB > 0 ? sub.dataLimitGB + ' GB' : 'غير محدود' }}</p>
            </div>
            <div>
              <p class="text-light-gray">المتبقي</p>
              <p class="font-semibold text-jazan-green">{{ getRemainingGB(sub.dataUsedBytes || 0, sub.dataLimitGB) }}</p>
            </div>
            <div v-if="sub.dataLimitGB > 0">
              <p class="text-light-gray">نسبة الاستهلاك</p>
              <p class="font-semibold" :class="getDataUsagePercent(sub.dataUsedBytes || 0, sub.dataLimitGB) >= 90 ? 'text-red-coral' : 'text-golden-sand-dark'">
                {{ getDataUsagePercent(sub.dataUsedBytes || 0, sub.dataLimitGB) }}%
              </p>
            </div>
          </div>
          <!-- Progress bar -->
          <div v-if="sub.dataLimitGB > 0" class="w-full bg-gray-200 rounded-full h-3">
            <div 
              class="h-3 rounded-full transition-all duration-300"
              :class="getDataUsagePercent(sub.dataUsedBytes || 0, sub.dataLimitGB) >= 90 ? 'bg-red-coral' : getDataUsagePercent(sub.dataUsedBytes || 0, sub.dataLimitGB) >= 70 ? 'bg-warning-yellow' : 'bg-jazan-green'"
              :style="{ width: getDataUsagePercent(sub.dataUsedBytes || 0, sub.dataLimitGB) + '%' }"
            ></div>
          </div>
          <p v-if="sub.dataLimitExceeded" class="text-red-coral text-sm mt-2 font-semibold">
            ⚠️ تم تجاوز حد البيانات - الاتصال معطل
          </p>
        </div>
      </div>
    </div>
    <div v-else class="bg-white rounded-xl shadow-md p-12 text-center text-light-gray border border-soft-beige">
      لا يوجد اشتراك حالياً
    </div>

    <!-- Network Info -->
    <div v-if="myProfile" class="bg-white rounded-xl shadow-md p-6 border border-soft-beige mt-6">
      <h3 class="text-lg font-semibold text-coastal-blue mb-4">معلومات الاتصال</h3>
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
