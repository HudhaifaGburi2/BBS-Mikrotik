<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { apiGet } from '@/services/http'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import type { DashboardStats } from '@/types'

const { formatCurrency } = useFormatters()
const isLoading = ref(false)
const stats = ref<DashboardStats>({ totalSubscribers: 0, activeSubscriptions: 0, monthlyRevenue: 0, overdueInvoices: 0 })

async function loadReports() {
  isLoading.value = true
  try {
    const res = await apiGet<DashboardStats>('/reports/dashboard-stats')
    if (res.data) stats.value = res.data
  } catch (err) {
    console.error('Error loading reports:', err)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => loadReports())
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-6">التقارير</h1>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
      <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <h3 class="text-lg font-semibold text-jazan-green mb-4">ملخص المشتركين</h3>
        <div class="space-y-3">
          <div class="flex justify-between"><span class="text-light-gray">إجمالي المشتركين:</span><span class="font-bold text-charcoal">{{ stats.totalSubscribers }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">الاشتراكات النشطة:</span><span class="font-bold text-teal">{{ stats.activeSubscriptions }}</span></div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <h3 class="text-lg font-semibold text-jazan-green mb-4">ملخص مالي</h3>
        <div class="space-y-3">
          <div class="flex justify-between"><span class="text-light-gray">الإيرادات الشهرية:</span><span class="font-bold text-teal">{{ formatCurrency(stats.monthlyRevenue) }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">فواتير متأخرة:</span><span class="font-bold text-red-coral">{{ stats.overdueInvoices }}</span></div>
        </div>
      </div>
    </div>

    <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
      <h3 class="text-lg font-semibold text-jazan-green mb-4">تقارير مفصلة</h3>
      <p class="text-light-gray text-center py-8">سيتم إضافة تقارير مفصلة مع رسوم بيانية قريباً</p>
    </div>
  </div>
</template>
