<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { apiGet } from '@/services/http'
import { useFormatters } from '@/composables/useFormatters'
import type { DashboardStats } from '@/types'
import AppLoader from '@/components/AppLoader.vue'

const { formatCurrency, formatDateTime } = useFormatters()

const stats = ref<DashboardStats>({ totalSubscribers: 0, activeSubscriptions: 0, monthlyRevenue: 0, overdueInvoices: 0 })
const lastSync = ref('')
const isLoading = ref(false)
let refreshInterval: ReturnType<typeof setInterval> | null = null

async function loadStats() {
  isLoading.value = true
  try {
    const res = await apiGet<DashboardStats>('/reports/dashboard-stats')
    if (res.data) stats.value = res.data
    lastSync.value = formatDateTime(new Date().toISOString())
  } catch (err) {
    console.error('Error loading dashboard stats:', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  loadStats()
  refreshInterval = setInterval(loadStats, 60000)
})

onUnmounted(() => {
  if (refreshInterval) clearInterval(refreshInterval)
})

const statCards: Array<{ key: keyof DashboardStats; label: string; color: string; icon: string; isCurrency?: boolean }> = [
  { key: 'totalSubscribers', label: 'إجمالي المشتركين', color: 'bg-coastal-blue', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' },
  { key: 'activeSubscriptions', label: 'الاشتراكات النشطة', color: 'bg-jazan-green', icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z' },
  { key: 'monthlyRevenue', label: 'الإيرادات الشهرية', color: 'bg-golden-sand', icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z', isCurrency: true },
  { key: 'overdueInvoices', label: 'فواتير متأخرة', color: 'bg-red-coral', icon: 'M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z' },
]
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">لوحة التحكم</h1>
      <span class="text-sm text-light-gray">آخر تحديث: {{ lastSync }}</span>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div v-for="card in statCards" :key="card.key" class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-light-gray">{{ card.label }}</p>
            <p class="text-2xl font-bold text-charcoal mt-1">
              {{ card.isCurrency ? formatCurrency(stats[card.key] as number) : stats[card.key] }}
            </p>
          </div>
          <div :class="[card.color, 'p-3 rounded-full']">
            <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="card.icon" />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
      <h2 class="text-lg font-semibold text-jazan-green mb-4">النشاط الأخير</h2>
      <p class="text-light-gray text-center py-8">سيتم عرض النشاط الأخير هنا</p>
    </div>
  </div>
</template>
