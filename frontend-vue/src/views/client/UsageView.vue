<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { apiGet } from '@/services/http'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'

interface UsageData {
  date: string
  downloadBytes: number
  uploadBytes: number
  totalBytes: number
}

const { formatBytes, formatDate } = useFormatters()
const isLoading = ref(false)
const usageData = ref<UsageData[]>([])

onMounted(async () => {
  isLoading.value = true
  try {
    const res = await apiGet<UsageData[]>('/usage/my')
    if (res.data) {
      usageData.value = res.data
    }
  } catch {
    // Usage data may not be available
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-6">استهلاكي</h1>

    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <table v-if="usageData.length" class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">التاريخ</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">تحميل</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">رفع</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الإجمالي</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-200">
          <tr v-for="u in usageData" :key="u.date" class="hover:bg-soft-beige-light">
            <td class="px-6 py-4 text-sm text-charcoal">{{ formatDate(u.date) }}</td>
            <td class="px-6 py-4 text-sm text-light-gray">{{ formatBytes(u.downloadBytes) }}</td>
            <td class="px-6 py-4 text-sm text-light-gray">{{ formatBytes(u.uploadBytes) }}</td>
            <td class="px-6 py-4 text-sm font-medium text-charcoal">{{ formatBytes(u.totalBytes) }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="text-center py-12 text-light-gray">لا توجد بيانات استهلاك متاحة</div>
    </div>
  </div>
</template>
