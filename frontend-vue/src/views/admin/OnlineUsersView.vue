<script setup lang="ts">
import { ref } from 'vue'
import { apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import type { ActiveSession } from '@/types'

const toast = useToastStore()
const { formatBytes } = useFormatters()

const sessions = ref<ActiveSession[]>([])
const isLoading = ref(false)
const hasLoaded = ref(false)
const errorMessage = ref('')

async function loadSessions() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    // Backend uses credentials from appsettings.json
    const res = await apiPost<{ success: boolean; data: ActiveSession[]; message: string }>('/mikrotik/active-sessions', {})
    hasLoaded.value = true
    if (res.data?.success && res.data.data) {
      sessions.value = res.data.data
    } else {
      sessions.value = []
      errorMessage.value = res.data?.message || 'لا توجد بيانات'
    }
  } catch (err: any) {
    hasLoaded.value = true
    sessions.value = []
    errorMessage.value = err.response?.data?.message || 'فشل الاتصال بجهاز MikroTik - تحقق من إعدادات الخادم'
  } finally {
    isLoading.value = false
  }
}

async function disconnectUser(username: string) {
  try {
    await apiPost('/mikrotik/disconnect-user', { username })
    toast.success(`تم قطع اتصال ${username}`)
    loadSessions()
  } catch {
    toast.error('فشل قطع الاتصال')
  }
}
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-coastal-blue">المستخدمون المتصلون</h1>
      <div class="flex items-center gap-2">
        <span v-if="hasLoaded" class="text-sm text-light-gray">{{ sessions.length }} متصل</span>
        <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:bg-coastal-blue-dark text-sm transition-colors" @click="loadSessions">
          {{ hasLoaded ? 'تحديث' : 'اتصال' }}
        </button>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <!-- Not connected yet -->
        <div v-if="!hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M5.636 18.364a9 9 0 010-12.728m12.728 0a9 9 0 010 12.728m-9.9-2.829a5 5 0 010-7.07m7.072 0a5 5 0 010 7.07M13 12a1 1 0 11-2 0 1 1 0 012 0z" />
          </svg>
          <p class="text-charcoal font-medium">أدخل بيانات الاتصال بجهاز MikroTik ثم اضغط "اتصال"</p>
          <p class="text-light-gray text-sm mt-1">لم يتم الاتصال بعد</p>
        </div>

        <!-- Has data -->
        <table v-else-if="sessions.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المستخدم</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">العنوان</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">مدة الاتصال</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">تحميل</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">رفع</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="s in sessions" :key="s.username" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-charcoal">{{ s.username }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ s.address }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ s.uptime }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ formatBytes(s.bytesIn) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-light-gray">{{ formatBytes(s.bytesOut) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <button class="text-red-coral hover:text-red-coral-dark font-medium" @click="disconnectUser(s.username)">قطع</button>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- No data after loading -->
        <div v-else-if="hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p class="text-charcoal font-medium">لا توجد بيانات</p>
          <p v-if="errorMessage" class="text-red-coral text-sm mt-1">{{ errorMessage }}</p>
          <p v-else class="text-light-gray text-sm mt-1">لا يوجد مستخدمون متصلون حالياً</p>
        </div>
      </div>
    </div>
  </div>
</template>
