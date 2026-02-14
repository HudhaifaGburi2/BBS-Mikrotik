<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSubscriptionsStore } from '@/stores/subscriptions'
import { useFormatters } from '@/composables/useFormatters'
import { apiGet } from '@/services/http'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import type { Subscriber } from '@/types'

const store = useSubscriptionsStore()
const { formatDate } = useFormatters()
const myProfile = ref<Subscriber | null>(null)

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
