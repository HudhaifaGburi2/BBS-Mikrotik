<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import type { ActiveSession } from '@/types'

const toast = useToastStore()
const { formatBytes } = useFormatters()

const sessions = ref<ActiveSession[]>([])
const isLoading = ref(false)
const hasLoaded = ref(false)
const errorMessage = ref('')
const searchQuery = ref('')
const autoRefresh = ref(false)
const lastRefresh = ref<Date | null>(null)
let refreshInterval: ReturnType<typeof setInterval> | null = null

// Disconnect confirmation
const confirmVisible = ref(false)
const pendingDisconnectName = ref('')

// Filtered sessions
const filteredSessions = computed(() => {
  if (!searchQuery.value) return sessions.value
  const query = searchQuery.value.toLowerCase()
  return sessions.value.filter(s => 
    s.name.toLowerCase().includes(query) ||
    s.address?.toLowerCase().includes(query)
  )
})

// Statistics
const stats = computed(() => {
  const totalUsers = sessions.value.length
  const totalBytesIn = sessions.value.reduce((sum, s) => sum + (s.bytesIn || 0), 0)
  const totalBytesOut = sessions.value.reduce((sum, s) => sum + (s.bytesOut || 0), 0)
  const totalBytes = totalBytesIn + totalBytesOut
  return { totalUsers, totalBytesIn, totalBytesOut, totalBytes }
})

// Sort sessions by total bytes (highest first)
const sortedSessions = computed(() => {
  return [...filteredSessions.value].sort((a, b) => {
    const totalA = (a.bytesIn || 0) + (a.bytesOut || 0)
    const totalB = (b.bytesIn || 0) + (b.bytesOut || 0)
    return totalB - totalA
  })
})

// Get usage percentage for progress bar (based on data limit if available, otherwise relative to top user)
function getUsagePercent(session: ActiveSession): number {
  const sessionTotal = (session.bytesIn || 0) + (session.bytesOut || 0)
  
  // If user has a data limit, calculate percentage of limit used
  if (session.limitBytesIn && session.limitBytesIn > 0) {
    return Math.min(100, Math.round((sessionTotal / session.limitBytesIn) * 100))
  }
  
  // Otherwise, calculate relative to top user
  if (sessions.value.length === 0) return 0
  const maxTotal = Math.max(...sessions.value.map(s => (s.bytesIn || 0) + (s.bytesOut || 0)))
  if (maxTotal === 0) return 0
  return Math.round((sessionTotal / maxTotal) * 100)
}

// Check if user has a data limit
function hasDataLimit(session: ActiveSession): boolean {
  return (session.limitBytesIn != null && session.limitBytesIn > 0) || 
         (session.limitBytesOut != null && session.limitBytesOut > 0)
}

// Get remaining bytes (limit - used)
function getRemainingBytes(session: ActiveSession): number {
  const used = (session.bytesIn || 0) + (session.bytesOut || 0)
  const limit = session.limitBytesIn || session.limitBytesOut || 0
  return Math.max(0, limit - used)
}

// Get data limit (use limitBytesIn as the total limit)
function getDataLimit(session: ActiveSession): number {
  return session.limitBytesIn || session.limitBytesOut || 0
}

// Format uptime for display
function formatUptime(uptime: string | null): string {
  if (!uptime) return '—'
  // MikroTik format: 1d2h3m4s or 2h3m4s or 3m4s
  return uptime
}

async function loadSessions() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    const res = await apiPost<{ success: boolean; data: ActiveSession[]; message: string }>('/mikrotik/active-sessions', {})
    hasLoaded.value = true
    lastRefresh.value = new Date()
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

function confirmDisconnect(username: string) {
  pendingDisconnectName.value = username
  confirmVisible.value = true
}

async function handleDisconnect() {
  confirmVisible.value = false
  const username = pendingDisconnectName.value
  try {
    await apiPost('/mikrotik/disconnect-user', { pppUsername: username })
    toast.success(`تم قطع اتصال ${username}`)
    loadSessions()
  } catch {
    toast.error('فشل قطع الاتصال')
  }
}

async function disconnectAll() {
  if (!confirm('هل أنت متأكد من قطع اتصال جميع المستخدمين؟')) return
  
  for (const session of sessions.value) {
    try {
      await apiPost('/mikrotik/disconnect-user', { pppUsername: session.name })
    } catch {
      // Continue with others
    }
  }
  toast.success('تم قطع اتصال جميع المستخدمين')
  loadSessions()
}

function toggleAutoRefresh() {
  autoRefresh.value = !autoRefresh.value
  if (autoRefresh.value) {
    refreshInterval = setInterval(loadSessions, 30000) // Refresh every 30 seconds
    toast.success('تم تفعيل التحديث التلقائي (كل 30 ثانية)')
  } else {
    if (refreshInterval) {
      clearInterval(refreshInterval)
      refreshInterval = null
    }
    toast.info('تم إيقاف التحديث التلقائي')
  }
}

onMounted(() => {
  loadSessions()
})

onUnmounted(() => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
  }
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-coastal-blue">المستخدمون المتصلون</h1>
        <p class="text-light-gray text-sm mt-1">مراقبة الجلسات النشطة واستهلاك البيانات في الوقت الفعلي</p>
      </div>
      <div class="flex items-center gap-2 flex-wrap">
        <button 
          v-if="hasLoaded && sessions.length > 0" 
          class="px-3 py-2 text-sm rounded-lg transition-colors flex items-center gap-2"
          :class="autoRefresh ? 'bg-jazan-green text-white' : 'bg-gray-100 text-charcoal hover:bg-gray-200'"
          @click="toggleAutoRefresh"
        >
          <svg class="w-4 h-4" :class="autoRefresh ? 'animate-spin' : ''" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          {{ autoRefresh ? 'إيقاف التحديث' : 'تحديث تلقائي' }}
        </button>
        <button 
          v-if="hasLoaded && sessions.length > 1" 
          class="px-3 py-2 bg-red-coral/10 text-red-coral rounded-lg hover:bg-red-coral/20 text-sm transition-colors"
          @click="disconnectAll"
        >
          قطع الكل
        </button>
        <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm transition-colors flex items-center gap-2" @click="loadSessions">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          {{ hasLoaded ? 'تحديث' : 'اتصال' }}
        </button>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div v-if="hasLoaded" class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">المتصلون الآن</p>
            <p class="text-2xl font-bold text-coastal-blue">{{ stats.totalUsers }}</p>
          </div>
          <div class="w-10 h-10 bg-coastal-blue/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-coastal-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">إجمالي التحميل ↓</p>
            <p class="text-2xl font-bold text-jazan-green">{{ formatBytes(stats.totalBytesIn) }}</p>
          </div>
          <div class="w-10 h-10 bg-jazan-green/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-jazan-green" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 14l-7 7m0 0l-7-7m7 7V3" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">إجمالي الرفع ↑</p>
            <p class="text-2xl font-bold text-teal">{{ formatBytes(stats.totalBytesOut) }}</p>
          </div>
          <div class="w-10 h-10 bg-teal/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-teal" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 10l7-7m0 0l7 7m-7-7v18" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">إجمالي الاستهلاك</p>
            <p class="text-2xl font-bold text-golden-sand-dark">{{ formatBytes(stats.totalBytes) }}</p>
          </div>
          <div class="w-10 h-10 bg-golden-sand/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-golden-sand-dark" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Last Refresh & Search -->
    <div v-if="hasLoaded && sessions.length" class="flex flex-col md:flex-row gap-4">
      <div class="flex-1 relative">
        <svg class="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-light-gray" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <input v-model="searchQuery" type="text" placeholder="بحث بالاسم أو العنوان..." class="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
      </div>
      <div class="flex items-center gap-4 text-sm text-light-gray">
        <span>{{ filteredSessions.length }} من {{ sessions.length }}</span>
        <span v-if="lastRefresh" class="flex items-center gap-1">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          آخر تحديث: {{ lastRefresh.toLocaleTimeString('ar-SA') }}
        </span>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <!-- Not connected yet -->
        <div v-if="!hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-16 w-16 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M5.636 18.364a9 9 0 010-12.728m12.728 0a9 9 0 010 12.728m-9.9-2.829a5 5 0 010-7.07m7.072 0a5 5 0 010 7.07M13 12a1 1 0 11-2 0 1 1 0 012 0z" />
          </svg>
          <p class="text-charcoal font-medium text-lg">الاتصال بجهاز MikroTik</p>
          <p class="text-light-gray text-sm mt-1">اضغط على زر "اتصال" لجلب قائمة المستخدمين المتصلين</p>
        </div>

        <!-- Has data -->
        <div v-else-if="sortedSessions.length" class="divide-y divide-gray-100">
          <div v-for="(s, index) in sortedSessions" :key="s.id || s.name" class="p-4 hover:bg-gray-50 transition-colors">
            <div class="flex flex-col md:flex-row md:items-center gap-4">
              <!-- User Info -->
              <div class="flex items-center gap-3 md:w-1/4">
                <div class="relative">
                  <div class="w-10 h-10 rounded-full bg-coastal-blue/10 flex items-center justify-center">
                    <svg class="w-5 h-5 text-coastal-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </div>
                  <span class="absolute -bottom-1 -right-1 w-3 h-3 bg-jazan-green rounded-full border-2 border-white"></span>
                </div>
                <div>
                  <p class="font-semibold text-charcoal">{{ s.name }}</p>
                  <p class="text-xs text-light-gray font-mono">{{ s.address || '—' }}</p>
                </div>
                <span v-if="index === 0 && sessions.length > 1" class="px-2 py-0.5 bg-golden-sand/20 text-golden-sand-dark text-xs rounded-full font-medium">
                  الأعلى استهلاكاً
                </span>
              </div>

              <!-- Uptime -->
              <div class="md:w-1/6">
                <p class="text-xs text-light-gray mb-1">مدة الاتصال</p>
                <p class="font-medium text-charcoal">{{ formatUptime(s.uptime) }}</p>
              </div>

              <!-- Data Usage -->
              <div class="flex-1">
                <div class="grid gap-4 mb-2" :class="hasDataLimit(s) ? 'grid-cols-4' : 'grid-cols-3'">
                  <div class="text-center">
                    <p class="text-xs text-light-gray mb-1">تحميل ↓</p>
                    <p class="font-bold text-jazan-green">{{ formatBytes(s.bytesIn || 0) }}</p>
                  </div>
                  <div class="text-center">
                    <p class="text-xs text-light-gray mb-1">رفع ↑</p>
                    <p class="font-bold text-teal">{{ formatBytes(s.bytesOut || 0) }}</p>
                  </div>
                  <div class="text-center">
                    <p class="text-xs text-light-gray mb-1">الإجمالي</p>
                    <p class="font-bold text-golden-sand-dark">{{ formatBytes((s.bytesIn || 0) + (s.bytesOut || 0)) }}</p>
                  </div>
                  <div v-if="hasDataLimit(s)" class="text-center">
                    <p class="text-xs text-light-gray mb-1">المتبقي</p>
                    <p class="font-bold" :class="getRemainingBytes(s) > 0 ? 'text-coastal-blue' : 'text-red-coral'">
                      {{ formatBytes(getRemainingBytes(s)) }}
                    </p>
                  </div>
                </div>
                <!-- Data Limit Info -->
                <div v-if="hasDataLimit(s)" class="flex items-center justify-between text-xs mb-1">
                  <span class="text-light-gray">حصة البيانات: {{ formatBytes(getDataLimit(s)) }}</span>
                  <span :class="getUsagePercent(s) >= 90 ? 'text-red-coral' : getUsagePercent(s) >= 70 ? 'text-warning-yellow' : 'text-jazan-green'">
                    {{ getUsagePercent(s) }}% مستهلك
                  </span>
                </div>
                <!-- Usage Progress Bar -->
                <div class="w-full bg-gray-200 rounded-full h-1.5">
                  <div 
                    class="h-1.5 rounded-full transition-all duration-300"
                    :class="hasDataLimit(s) ? (getUsagePercent(s) >= 90 ? 'bg-red-coral' : getUsagePercent(s) >= 70 ? 'bg-warning-yellow' : 'bg-jazan-green') : 'bg-gradient-to-r from-jazan-green to-teal'"
                    :style="{ width: getUsagePercent(s) + '%' }"
                  ></div>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex items-center gap-2 md:w-auto">
                <button 
                  class="px-3 py-1.5 bg-red-coral/10 text-red-coral rounded-lg hover:bg-red-coral/20 text-sm transition-colors flex items-center gap-1"
                  @click="confirmDisconnect(s.name)"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
                  </svg>
                  قطع الاتصال
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- No data after loading -->
        <div v-else-if="hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p class="text-charcoal font-medium">{{ searchQuery ? 'لا توجد نتائج' : 'لا يوجد مستخدمون متصلون' }}</p>
          <p v-if="errorMessage" class="text-red-coral text-sm mt-1">{{ errorMessage }}</p>
          <p v-else class="text-light-gray text-sm mt-1">{{ searchQuery ? 'جرب البحث بكلمات مختلفة' : 'لا يوجد مستخدمون متصلون حالياً على جهاز MikroTik' }}</p>
        </div>
      </div>
    </div>

    <!-- Disconnect Confirmation Dialog -->
    <ConfirmDialog
      :visible="confirmVisible"
      title="تأكيد قطع الاتصال"
      :message="`هل أنت متأكد من قطع اتصال المستخدم '${pendingDisconnectName}'؟`"
      confirm-text="قطع الاتصال"
      cancel-text="إلغاء"
      @confirm="handleDisconnect"
      @cancel="confirmVisible = false"
    />
  </div>
</template>
