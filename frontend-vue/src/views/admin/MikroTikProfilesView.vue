<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import AppLoader from '@/components/AppLoader.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

interface PppProfile {
  id: string
  name: string
  localAddress: string | null
  remoteAddress: string | null
  rateLimit: string | null
  onlyOne: boolean
}

const toast = useToastStore()
const profiles = ref<PppProfile[]>([])
const isLoading = ref(false)
const showAddForm = ref(false)
const hasLoaded = ref(false)
const errorMessage = ref('')
const searchQuery = ref('')

const newProfile = ref({ 
  name: '', 
  localAddress: '', 
  remoteAddress: '', 
  rateLimit: '',
  downloadSpeed: '',
  uploadSpeed: '',
  speedUnit: 'M'
})

// Edit state
const showEditModal = ref(false)
const editingProfile = ref<PppProfile | null>(null)
const editForm = ref({ 
  rateLimit: '', 
  localAddress: '', 
  remoteAddress: '',
  downloadSpeed: '',
  uploadSpeed: '',
  speedUnit: 'M'
})

// Delete confirmation
const confirmVisible = ref(false)
const pendingDeleteName = ref('')

// Filtered profiles
const filteredProfiles = computed(() => {
  if (!searchQuery.value) return profiles.value
  const query = searchQuery.value.toLowerCase()
  return profiles.value.filter(p => 
    p.name.toLowerCase().includes(query) ||
    p.rateLimit?.toLowerCase().includes(query)
  )
})

// Statistics
const stats = computed(() => {
  const total = profiles.value.length
  const withRateLimit = profiles.value.filter(p => p.rateLimit).length
  const withoutRateLimit = profiles.value.filter(p => !p.rateLimit).length
  return { total, withRateLimit, withoutRateLimit }
})

// Parse rate limit to get download/upload speeds
function parseRateLimit(rateLimit: string | null): { download: string; upload: string } {
  if (!rateLimit) return { download: '—', upload: '—' }
  const parts = rateLimit.split('/')
  return {
    download: parts[0] || '—',
    upload: parts[1] || parts[0] || '—'
  }
}

// Format speed for display with color coding
function getSpeedClass(speed: string): string {
  if (speed === '—') return 'text-light-gray'
  const num = parseInt(speed)
  if (isNaN(num)) return 'text-charcoal'
  if (speed.includes('G') || num >= 100) return 'text-jazan-green'
  if (num >= 50) return 'text-teal'
  if (num >= 10) return 'text-coastal-blue'
  return 'text-golden-sand-dark'
}

// Build rate limit string from separate fields
function buildRateLimit(download: string, upload: string, unit: string): string {
  if (!download && !upload) return ''
  const dl = download ? `${download}${unit}` : ''
  const ul = upload ? `${upload}${unit}` : dl
  return `${dl}/${ul}`
}

async function loadProfiles() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    // Backend uses credentials from appsettings.json
    const res = await apiPost<{ success: boolean; data: PppProfile[]; message: string }>('/mikrotik/ppp-profiles', {})
    hasLoaded.value = true
    if (res.data?.success && res.data.data) {
      profiles.value = res.data.data
    } else {
      profiles.value = []
      errorMessage.value = res.data?.message || 'لا توجد بيانات'
    }
  } catch (err: any) {
    hasLoaded.value = true
    profiles.value = []
    errorMessage.value = err.response?.data?.message || 'فشل الاتصال بجهاز MikroTik - تحقق من إعدادات الخادم'
  } finally {
    isLoading.value = false
  }
}

async function addProfile() {
  if (!newProfile.value.name) {
    toast.error('الرجاء إدخال اسم البروفايل')
    return
  }
  
  // Build rate limit from speed fields or use direct input
  let rateLimit = newProfile.value.rateLimit
  if (newProfile.value.downloadSpeed || newProfile.value.uploadSpeed) {
    rateLimit = buildRateLimit(newProfile.value.downloadSpeed, newProfile.value.uploadSpeed, newProfile.value.speedUnit)
  }
  
  try {
    const payload = {
      profileName: newProfile.value.name,
      rateLimit: rateLimit || null,
      localAddress: newProfile.value.localAddress || null,
      remoteAddress: newProfile.value.remoteAddress || null
    }
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-profiles/add', payload)
    if (res.data?.success) {
      toast.success('تم إضافة البروفايل بنجاح')
      showAddForm.value = false
      newProfile.value = { name: '', localAddress: '', remoteAddress: '', rateLimit: '', downloadSpeed: '', uploadSpeed: '', speedUnit: 'M' }
      loadProfiles()
    } else {
      toast.error(res.data?.message || 'فشل إضافة البروفايل')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل إضافة البروفايل')
  }
}

function confirmDelete(name: string) {
  pendingDeleteName.value = name
  confirmVisible.value = true
}

async function handleDelete() {
  confirmVisible.value = false
  const name = pendingDeleteName.value
  try {
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-profiles/delete', { profileName: name })
    if (res.data?.success) {
      toast.success('تم حذف البروفايل')
      loadProfiles()
    } else {
      toast.error(res.data?.message || 'فشل حذف البروفايل')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل حذف البروفايل')
  }
}

function openEditModal(profile: PppProfile) {
  editingProfile.value = profile
  const speeds = parseRateLimit(profile.rateLimit)
  editForm.value = {
    rateLimit: profile.rateLimit || '',
    localAddress: profile.localAddress || '',
    remoteAddress: profile.remoteAddress || '',
    downloadSpeed: speeds.download !== '—' ? speeds.download.replace(/[MKG]/gi, '') : '',
    uploadSpeed: speeds.upload !== '—' ? speeds.upload.replace(/[MKG]/gi, '') : '',
    speedUnit: profile.rateLimit?.includes('G') ? 'G' : profile.rateLimit?.includes('K') ? 'K' : 'M'
  }
  showEditModal.value = true
}

function closeEditModal() {
  editingProfile.value = null
  showEditModal.value = false
  editForm.value = { rateLimit: '', localAddress: '', remoteAddress: '', downloadSpeed: '', uploadSpeed: '', speedUnit: 'M' }
}

async function saveEdit() {
  if (!editingProfile.value) return
  
  // Build rate limit from speed fields or use direct input
  let rateLimit = editForm.value.rateLimit
  if (editForm.value.downloadSpeed || editForm.value.uploadSpeed) {
    rateLimit = buildRateLimit(editForm.value.downloadSpeed, editForm.value.uploadSpeed, editForm.value.speedUnit)
  }
  
  try {
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-profiles/update', {
      profileName: editingProfile.value.name,
      rateLimit: rateLimit
    })
    if (res.data?.success) {
      toast.success('تم تحديث البروفايل بنجاح')
      closeEditModal()
      loadProfiles()
    } else {
      toast.error(res.data?.message || 'فشل تحديث البروفايل')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تحديث البروفايل')
  }
}

// Auto-load on mount
onMounted(() => {
  loadProfiles()
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-coastal-blue">إدارة بروفايلات السرعة</h1>
        <p class="text-light-gray text-sm mt-1">إدارة باقات السرعة (PPP Profiles) على MikroTik</p>
      </div>
      <div class="flex items-center gap-2">
        <button v-if="hasLoaded" class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm flex items-center gap-2" @click="showAddForm = !showAddForm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          {{ showAddForm ? 'إلغاء' : 'إضافة بروفايل' }}
        </button>
        <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm flex items-center gap-2" @click="loadProfiles">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          {{ hasLoaded ? 'تحديث' : 'اتصال' }}
        </button>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div v-if="hasLoaded" class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">إجمالي البروفايلات</p>
            <p class="text-2xl font-bold text-coastal-blue">{{ stats.total }}</p>
          </div>
          <div class="w-10 h-10 bg-coastal-blue/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-coastal-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">بحد سرعة</p>
            <p class="text-2xl font-bold text-jazan-green">{{ stats.withRateLimit }}</p>
          </div>
          <div class="w-10 h-10 bg-jazan-green/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-jazan-green" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">بدون حد سرعة</p>
            <p class="text-2xl font-bold text-warning-yellow">{{ stats.withoutRateLimit }}</p>
          </div>
          <div class="w-10 h-10 bg-warning-yellow/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-warning-yellow" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Profile Form -->
    <div v-if="showAddForm" class="bg-white rounded-xl shadow-md p-6 border-2 border-jazan-green">
      <h3 class="text-lg font-semibold text-jazan-green mb-4 flex items-center gap-2">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
        إضافة بروفايل سرعة جديد
      </h3>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">اسم البروفايل *</label>
          <input v-model="newProfile.name" placeholder="مثال: 10Mbps" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">سرعة التحميل</label>
          <div class="flex gap-2">
            <input v-model="newProfile.downloadSpeed" type="number" min="0" placeholder="10" class="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
            <select v-model="newProfile.speedUnit" class="border border-gray-300 rounded-lg px-2 py-2 text-sm focus:ring-2 focus:ring-jazan-green">
              <option value="K">Kbps</option>
              <option value="M">Mbps</option>
              <option value="G">Gbps</option>
            </select>
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">سرعة الرفع</label>
          <div class="flex gap-2">
            <input v-model="newProfile.uploadSpeed" type="number" min="0" placeholder="10" class="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
            <span class="border border-gray-200 bg-gray-50 rounded-lg px-3 py-2 text-sm text-light-gray">{{ newProfile.speedUnit === 'K' ? 'Kbps' : newProfile.speedUnit === 'G' ? 'Gbps' : 'Mbps' }}</span>
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">أو أدخل حد السرعة مباشرة</label>
          <input v-model="newProfile.rateLimit" placeholder="مثال: 10M/5M" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
          <p class="text-xs text-light-gray mt-1">صيغة: تحميل/رفع (مثال: 10M/5M)</p>
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">العنوان المحلي</label>
          <input v-model="newProfile.localAddress" placeholder="اختياري" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div class="flex items-end">
          <button class="w-full px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm font-medium" @click="addProfile">
            إضافة البروفايل
          </button>
        </div>
      </div>
    </div>

    <!-- Search -->
    <div v-if="hasLoaded && profiles.length" class="flex flex-col md:flex-row gap-4">
      <div class="flex-1 relative">
        <svg class="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-light-gray" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <input v-model="searchQuery" type="text" placeholder="بحث بالاسم أو السرعة..." class="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
      </div>
      <div class="flex items-center gap-2">
        <span class="text-sm text-light-gray">{{ filteredProfiles.length }} من {{ profiles.length }}</span>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <!-- Not connected yet -->
        <div v-if="!hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-16 w-16 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          </svg>
          <p class="text-charcoal font-medium text-lg">الاتصال بجهاز MikroTik</p>
          <p class="text-light-gray text-sm mt-1">اضغط على زر "اتصال" لجلب قائمة البروفايلات</p>
        </div>

        <!-- Has data - Card Grid View -->
        <div v-else-if="filteredProfiles.length" class="p-4">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="p in filteredProfiles" :key="p.name" class="bg-gray-50 rounded-xl p-4 border border-gray-200 hover:border-coastal-blue hover:shadow-md transition-all">
              <!-- Profile Header -->
              <div class="flex items-start justify-between mb-3">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-full flex items-center justify-center" :class="p.rateLimit ? 'bg-jazan-green/10' : 'bg-warning-yellow/10'">
                    <svg class="w-5 h-5" :class="p.rateLimit ? 'text-jazan-green' : 'text-warning-yellow'" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  </div>
                  <div>
                    <h4 class="font-semibold text-charcoal">{{ p.name }}</h4>
                    <p class="text-xs text-light-gray">PPP Profile</p>
                  </div>
                </div>
                <div class="flex items-center gap-1">
                  <button class="p-1.5 text-coastal-blue hover:bg-coastal-blue/10 rounded-lg transition-colors" title="تعديل" @click="openEditModal(p)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>
                  <button class="p-1.5 text-red-coral hover:bg-red-100 rounded-lg transition-colors" title="حذف" @click="confirmDelete(p.name)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </div>

              <!-- Speed Info -->
              <div class="bg-white rounded-lg p-3 mb-3">
                <div v-if="p.rateLimit" class="grid grid-cols-2 gap-3">
                  <div class="text-center">
                    <p class="text-xs text-light-gray mb-1">تحميل ↓</p>
                    <p class="text-lg font-bold" :class="getSpeedClass(parseRateLimit(p.rateLimit).download)">
                      {{ parseRateLimit(p.rateLimit).download }}
                    </p>
                  </div>
                  <div class="text-center">
                    <p class="text-xs text-light-gray mb-1">رفع ↑</p>
                    <p class="text-lg font-bold" :class="getSpeedClass(parseRateLimit(p.rateLimit).upload)">
                      {{ parseRateLimit(p.rateLimit).upload }}
                    </p>
                  </div>
                </div>
                <div v-else class="text-center py-2">
                  <span class="text-warning-yellow text-sm font-medium">⚠️ بدون حد سرعة</span>
                </div>
              </div>

              <!-- Additional Info -->
              <div class="text-xs text-light-gray space-y-1">
                <div v-if="p.localAddress" class="flex justify-between">
                  <span>العنوان المحلي:</span>
                  <span class="font-mono text-charcoal">{{ p.localAddress }}</span>
                </div>
                <div v-if="p.remoteAddress" class="flex justify-between">
                  <span>العنوان البعيد:</span>
                  <span class="font-mono text-charcoal">{{ p.remoteAddress }}</span>
                </div>
                <div class="flex justify-between">
                  <span>اتصال واحد فقط:</span>
                  <span :class="p.onlyOne ? 'text-jazan-green' : 'text-light-gray'">{{ p.onlyOne ? 'نعم' : 'لا' }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- No data after loading -->
        <div v-else-if="hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p class="text-charcoal font-medium">{{ searchQuery ? 'لا توجد نتائج' : 'لا توجد بيانات' }}</p>
          <p v-if="errorMessage" class="text-red-coral text-sm mt-1">{{ errorMessage }}</p>
          <p v-else class="text-light-gray text-sm mt-1">{{ searchQuery ? 'جرب البحث بكلمات مختلفة' : 'لا يوجد بروفايلات PPP في جهاز MikroTik' }}</p>
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <div v-if="showEditModal && editingProfile" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-xl shadow-xl max-w-lg w-full p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-semibold text-coastal-blue">تعديل البروفايل: {{ editingProfile.name }}</h3>
          <button class="text-light-gray hover:text-charcoal" @click="closeEditModal">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-charcoal mb-1">سرعة التحميل</label>
              <div class="flex gap-2">
                <input v-model="editForm.downloadSpeed" type="number" min="0" placeholder="10" class="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
                <select v-model="editForm.speedUnit" class="border border-gray-300 rounded-lg px-2 py-2 text-sm focus:ring-2 focus:ring-coastal-blue">
                  <option value="K">Kbps</option>
                  <option value="M">Mbps</option>
                  <option value="G">Gbps</option>
                </select>
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-charcoal mb-1">سرعة الرفع</label>
              <input v-model="editForm.uploadSpeed" type="number" min="0" placeholder="10" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">أو أدخل حد السرعة مباشرة</label>
            <input v-model="editForm.rateLimit" placeholder="مثال: 10M/5M" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
            <p class="text-xs text-light-gray mt-1">صيغة: تحميل/رفع (مثال: 10M/5M)</p>
          </div>
        </div>
        <div class="flex justify-end gap-3 mt-6">
          <button class="px-4 py-2 text-light-gray hover:text-charcoal text-sm" @click="closeEditModal">إلغاء</button>
          <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm" @click="saveEdit">حفظ التغييرات</button>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog
      :visible="confirmVisible"
      title="تأكيد الحذف"
      :message="`هل أنت متأكد من حذف البروفايل '${pendingDeleteName}'؟ هذا الإجراء لا يمكن التراجع عنه.`"
      confirm-text="حذف"
      cancel-text="إلغاء"
      @confirm="handleDelete"
      @cancel="confirmVisible = false"
    />
  </div>
</template>
