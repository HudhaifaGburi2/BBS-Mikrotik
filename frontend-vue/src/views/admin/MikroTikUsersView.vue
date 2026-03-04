<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import { useMikroTikStore } from '@/stores/mikrotik'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

interface PppUser {
  id: string
  name: string
  password: string
  profile: string
  service: string
  disabled: boolean
  remoteAddress: string | null
  localAddress: string | null
  callerId: string | null
  comment: string | null
  limitBytesIn: number
  limitBytesOut: number
  limitBytesTotal: number
  isOnline: boolean
  // Enriched fields from SQL
  subscriptionId?: string | null
  subscriptionStatus?: string | null
  planName?: string | null
  planDataLimitGB?: number
  planDataLimitBytes?: number
  dataUsedBytes?: number
  dataRemainingBytes?: number
  dataUsagePercent?: number
  isUnlimited?: boolean
  dataLimitExceeded?: boolean
  isSuspended?: boolean
}

const toast = useToastStore()
const mikroTikStore = useMikroTikStore()
const { formatBytes } = useFormatters()

// Filter and search
const searchQuery = ref('')
const filterStatus = ref<'all' | 'online' | 'offline' | 'disabled'>('all')

const showAddForm = ref(false)
const newUser = ref({ 
  name: '', 
  password: '', 
  profile: 'default', 
  service: 'pppoe',
  limitGB: 0,
  comment: ''
})

// Edit modal
const showEditModal = ref(false)
const editingUser = ref<PppUser | null>(null)
const editForm = ref({
  newPassword: '',
  newProfile: '',
  limitGB: 0,
  comment: ''
})

// Delete confirmation
const confirmVisible = ref(false)
const pendingDeleteName = ref('')
const confirmAction = ref<'delete' | 'disconnect' | 'resetQuota'>('delete')

// Computed from store
const users = computed(() => mikroTikStore.pppUsers as PppUser[])
const isLoading = computed(() => mikroTikStore.isLoading)
const errorMessage = computed(() => mikroTikStore.errorMessage)
const hasLoaded = computed(() => mikroTikStore.isConnected || mikroTikStore.pppUsers.length > 0)

// Filtered users
const filteredUsers = computed(() => {
  let result = users.value
  
  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(u => 
      u.name.toLowerCase().includes(query) ||
      u.profile.toLowerCase().includes(query) ||
      u.comment?.toLowerCase().includes(query)
    )
  }
  
  // Status filter
  if (filterStatus.value === 'online') {
    result = result.filter(u => u.isOnline)
  } else if (filterStatus.value === 'offline') {
    result = result.filter(u => !u.isOnline && !u.disabled)
  } else if (filterStatus.value === 'disabled') {
    result = result.filter(u => u.disabled)
  }
  
  return result
})

// Statistics
const stats = computed(() => ({
  total: users.value.length,
  online: users.value.filter(u => u.isOnline).length,
  offline: users.value.filter(u => !u.isOnline && !u.disabled).length,
  disabled: users.value.filter(u => u.disabled).length,
  withQuota: users.value.filter(u => u.limitBytesTotal > 0).length
}))

async function loadUsers(force = false) {
  await mikroTikStore.fetchPppUsers(force)
}

async function addUser() {
  if (!newUser.value.name || !newUser.value.password) {
    toast.error('الرجاء إدخال اسم المستخدم وكلمة المرور')
    return
  }
  try {
    const limitBytes = newUser.value.limitGB * 1024 * 1024 * 1024
    const payload = {
      pppUsername: newUser.value.name,
      pppPassword: newUser.value.password,
      profile: newUser.value.profile,
      service: newUser.value.service
    }
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-users/add', payload)
    if (res.data?.success) {
      // If quota is set, update the user
      if (limitBytes > 0) {
        await apiPost('/mikrotik/ppp-users/update', {
          pppUsername: newUser.value.name,
          limitBytesIn: limitBytes,
          limitBytesOut: limitBytes,
          comment: newUser.value.comment
        })
      }
      toast.success('تم إضافة المستخدم بنجاح')
      showAddForm.value = false
      newUser.value = { name: '', password: '', profile: 'default', service: 'pppoe', limitGB: 0, comment: '' }
      await loadUsers(true)
    } else {
      toast.error(res.data?.message || 'فشل إضافة المستخدم')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل إضافة المستخدم')
  }
}

function openEditModal(user: PppUser) {
  editingUser.value = user
  editForm.value = {
    newPassword: '',
    newProfile: user.profile,
    limitGB: Math.round(user.limitBytesTotal / (1024 * 1024 * 1024)),
    comment: user.comment || ''
  }
  showEditModal.value = true
}

async function saveEdit() {
  if (!editingUser.value) return
  
  try {
    const limitBytes = editForm.value.limitGB * 1024 * 1024 * 1024
    const payload: Record<string, unknown> = {
      pppUsername: editingUser.value.name
    }
    
    if (editForm.value.newPassword) payload.newPassword = editForm.value.newPassword
    if (editForm.value.newProfile !== editingUser.value.profile) payload.newProfile = editForm.value.newProfile
    payload.limitBytesIn = limitBytes
    payload.limitBytesOut = limitBytes
    payload.comment = editForm.value.comment
    
    const res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-users/update', payload)
    if (res.data?.success) {
      toast.success('تم تحديث بيانات المستخدم بنجاح')
      showEditModal.value = false
      editingUser.value = null
      await loadUsers(true)
    } else {
      toast.error(res.data?.message || 'فشل تحديث المستخدم')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تحديث المستخدم')
  }
}

function confirmDelete(username: string) {
  pendingDeleteName.value = username
  confirmAction.value = 'delete'
  confirmVisible.value = true
}

function confirmDisconnect(username: string) {
  pendingDeleteName.value = username
  confirmAction.value = 'disconnect'
  confirmVisible.value = true
}

function confirmResetQuota(username: string) {
  pendingDeleteName.value = username
  confirmAction.value = 'resetQuota'
  confirmVisible.value = true
}

async function handleConfirm() {
  confirmVisible.value = false
  const username = pendingDeleteName.value
  
  try {
    let res
    if (confirmAction.value === 'delete') {
      res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-users/delete', { pppUsername: username })
      if (res.data?.success) toast.success('تم حذف المستخدم')
    } else if (confirmAction.value === 'disconnect') {
      res = await apiPost<{ success: boolean; message: string }>('/mikrotik/disconnect-user', { pppUsername: username })
      if (res.data?.success) toast.success('تم قطع اتصال المستخدم')
    } else if (confirmAction.value === 'resetQuota') {
      res = await apiPost<{ success: boolean; message: string }>('/mikrotik/ppp-users/reset-quota', { pppUsername: username })
      if (res.data?.success) toast.success('تم إعادة تعيين حصة البيانات')
    }
    
    if (res?.data?.success) {
      await loadUsers(true)
    } else {
      toast.error(res?.data?.message || 'فشل تنفيذ العملية')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تنفيذ العملية')
  }
}

async function toggleUser(username: string, disabled: boolean) {
  const endpoint = disabled ? '/mikrotik/ppp-users/activate' : '/mikrotik/ppp-users/deactivate'
  try {
    const res = await apiPost<{ success: boolean; message: string }>(endpoint, { pppUsername: username })
    if (res.data?.success) {
      toast.success(disabled ? 'تم تفعيل المستخدم' : 'تم تعطيل المستخدم')
      await loadUsers(true)
    } else {
      toast.error(res.data?.message || 'فشل تغيير حالة المستخدم')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تغيير حالة المستخدم')
  }
}

function getConfirmMessage() {
  if (confirmAction.value === 'delete') {
    return `هل أنت متأكد من حذف المستخدم '${pendingDeleteName.value}'؟ هذا الإجراء لا يمكن التراجع عنه.`
  } else if (confirmAction.value === 'disconnect') {
    return `هل أنت متأكد من قطع اتصال المستخدم '${pendingDeleteName.value}'؟`
  } else {
    return `هل أنت متأكد من إعادة تعيين حصة البيانات للمستخدم '${pendingDeleteName.value}'؟`
  }
}

function getConfirmTitle() {
  if (confirmAction.value === 'delete') return 'تأكيد الحذف'
  if (confirmAction.value === 'disconnect') return 'تأكيد قطع الاتصال'
  return 'تأكيد إعادة التعيين'
}

// Auto-load on mount
onMounted(() => {
  loadUsers()
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-coastal-blue">إدارة مستخدمي PPP</h1>
        <p class="text-light-gray text-sm mt-1">إدارة احترافية لمشتركي PPPoE على MikroTik</p>
      </div>
      <div class="flex items-center gap-2">
        <button v-if="hasLoaded" class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm flex items-center gap-2" @click="showAddForm = !showAddForm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          {{ showAddForm ? 'إلغاء' : 'إضافة مستخدم' }}
        </button>
        <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm flex items-center gap-2" @click="loadUsers(true)">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          {{ hasLoaded ? 'تحديث' : 'اتصال' }}
        </button>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div v-if="hasLoaded" class="grid grid-cols-2 md:grid-cols-5 gap-4">
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige cursor-pointer transition-all hover:shadow-md" :class="filterStatus === 'all' ? 'ring-2 ring-coastal-blue' : ''" @click="filterStatus = 'all'">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">إجمالي المستخدمين</p>
            <p class="text-2xl font-bold text-coastal-blue">{{ stats.total }}</p>
          </div>
          <div class="w-10 h-10 bg-coastal-blue/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-coastal-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige cursor-pointer transition-all hover:shadow-md" :class="filterStatus === 'online' ? 'ring-2 ring-jazan-green' : ''" @click="filterStatus = 'online'">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">متصل الآن</p>
            <p class="text-2xl font-bold text-jazan-green">{{ stats.online }}</p>
          </div>
          <div class="w-10 h-10 bg-jazan-green/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-jazan-green" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5.636 18.364a9 9 0 010-12.728m12.728 0a9 9 0 010 12.728m-9.9-2.829a5 5 0 010-7.07m7.072 0a5 5 0 010 7.07M13 12a1 1 0 11-2 0 1 1 0 012 0z" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige cursor-pointer transition-all hover:shadow-md" :class="filterStatus === 'offline' ? 'ring-2 ring-golden-sand' : ''" @click="filterStatus = 'offline'">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">غير متصل</p>
            <p class="text-2xl font-bold text-golden-sand-dark">{{ stats.offline }}</p>
          </div>
          <div class="w-10 h-10 bg-golden-sand/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-golden-sand-dark" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21M15.536 8.464a5 5 0 010 7.072m0 0l-2.829-2.829m-4.243 2.829a4.978 4.978 0 01-1.414-2.83m-1.414 5.658a9 9 0 01-2.167-9.238m7.824 2.167a1 1 0 111.414 1.414m-1.414-1.414L3 3" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige cursor-pointer transition-all hover:shadow-md" :class="filterStatus === 'disabled' ? 'ring-2 ring-red-coral' : ''" @click="filterStatus = 'disabled'">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">معطل</p>
            <p class="text-2xl font-bold text-red-coral">{{ stats.disabled }}</p>
          </div>
          <div class="w-10 h-10 bg-red-coral/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-red-coral" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
            </svg>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl shadow-sm p-4 border border-soft-beige">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-light-gray text-xs">بحصة بيانات</p>
            <p class="text-2xl font-bold text-teal">{{ stats.withQuota }}</p>
          </div>
          <div class="w-10 h-10 bg-teal/10 rounded-full flex items-center justify-center">
            <svg class="w-5 h-5 text-teal" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Add User Form -->
    <div v-if="showAddForm" class="bg-white rounded-xl shadow-md p-6 border-2 border-jazan-green">
      <h3 class="text-lg font-semibold text-jazan-green mb-4 flex items-center gap-2">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
        </svg>
        إضافة مستخدم PPP جديد
      </h3>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">اسم المستخدم *</label>
          <input v-model="newUser.name" placeholder="مثال: user_001" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">كلمة المرور *</label>
          <input v-model="newUser.password" type="password" placeholder="كلمة مرور قوية" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">البروفايل (السرعة)</label>
          <input v-model="newUser.profile" placeholder="default" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">حصة البيانات (GB)</label>
          <input v-model.number="newUser.limitGB" type="number" min="0" placeholder="0 = غير محدود" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-1">ملاحظات</label>
          <input v-model="newUser.comment" placeholder="ملاحظات اختيارية" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
        </div>
        <div class="flex items-end">
          <button class="w-full px-4 py-2 bg-jazan-green text-white rounded-lg hover:opacity-90 text-sm font-medium" @click="addUser">
            إضافة المستخدم
          </button>
        </div>
      </div>
    </div>

    <!-- Search and Filter -->
    <div v-if="hasLoaded && users.length" class="flex flex-col md:flex-row gap-4">
      <div class="flex-1 relative">
        <svg class="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-light-gray" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <input v-model="searchQuery" type="text" placeholder="بحث بالاسم أو البروفايل..." class="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
      </div>
      <div class="flex items-center gap-2">
        <span class="text-sm text-light-gray">{{ filteredUsers.length }} من {{ users.length }}</span>
        <button v-if="filterStatus !== 'all'" class="text-sm text-coastal-blue hover:underline" @click="filterStatus = 'all'">
          إظهار الكل
        </button>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white rounded-xl shadow-md overflow-hidden border border-soft-beige">
      <div class="overflow-x-auto">
        <!-- Not connected yet -->
        <div v-if="!hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-16 w-16 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
          </svg>
          <p class="text-charcoal font-medium text-lg">الاتصال بجهاز MikroTik</p>
          <p class="text-light-gray text-sm mt-1">اضغط على زر "اتصال" لجلب قائمة المستخدمين</p>
        </div>

        <!-- Has data -->
        <table v-else-if="filteredUsers.length" class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الاتصال</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">المستخدم</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">البروفايل</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">حصة البيانات</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الباقة</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">الحالة</th>
              <th class="px-4 py-3 text-right text-xs font-medium text-coastal-blue uppercase">إجراءات</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="u in filteredUsers" :key="u.name" class="hover:bg-gray-50 transition-colors">
              <!-- Status -->
              <td class="px-4 py-3 whitespace-nowrap">
                <div class="flex items-center gap-2">
                  <span v-if="u.disabled" class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-coral">
                    <span class="w-2 h-2 rounded-full bg-red-coral mr-1"></span>
                    معطل
                  </span>
                  <span v-else-if="u.isOnline" class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-jazan-green">
                    <span class="w-2 h-2 rounded-full bg-jazan-green mr-1 animate-pulse"></span>
                    متصل
                  </span>
                  <span v-else class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-light-gray">
                    <span class="w-2 h-2 rounded-full bg-gray-400 mr-1"></span>
                    غير متصل
                  </span>
                </div>
              </td>
              <!-- User -->
              <td class="px-4 py-3 whitespace-nowrap">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-8 rounded-full bg-coastal-blue/10 flex items-center justify-center">
                    <svg class="w-4 h-4 text-coastal-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-charcoal">{{ u.name }}</p>
                    <p class="text-xs text-light-gray">{{ u.service }}</p>
                  </div>
                </div>
              </td>
              <!-- Profile -->
              <td class="px-4 py-3 whitespace-nowrap">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-coastal-blue/10 text-coastal-blue">
                  {{ u.profile }}
                </span>
              </td>
              <!-- Quota & Usage -->
              <td class="px-4 py-3">
                <div v-if="u.isUnlimited || (u.planDataLimitGB === 0 && u.limitBytesTotal === 0)" class="text-sm">
                  <span class="text-light-gray">غير محدود</span>
                </div>
                <div v-else class="text-sm space-y-1">
                  <div class="flex items-center gap-2">
                    <span class="text-xs text-light-gray">الحصة:</span>
                    <span class="font-medium text-charcoal">{{ formatBytes(u.planDataLimitBytes || u.limitBytesTotal) }}</span>
                  </div>
                  <div class="flex items-center gap-2">
                    <span class="text-xs text-light-gray">المستخدم:</span>
                    <span class="font-medium text-golden-sand-dark">{{ formatBytes(u.dataUsedBytes || 0) }}</span>
                  </div>
                  <div class="flex items-center gap-2">
                    <span class="text-xs text-light-gray">المتبقي:</span>
                    <span class="font-medium" :class="(u.dataRemainingBytes || 0) > 0 ? 'text-jazan-green' : 'text-red-coral'">
                      {{ formatBytes(u.dataRemainingBytes || 0) }}
                    </span>
                  </div>
                  <!-- Usage Progress Bar -->
                  <div class="w-full bg-gray-200 rounded-full h-1.5 mt-1">
                    <div 
                      class="h-1.5 rounded-full transition-all duration-300"
                      :class="(u.dataUsagePercent || 0) >= 90 ? 'bg-red-coral' : (u.dataUsagePercent || 0) >= 70 ? 'bg-warning-yellow' : 'bg-jazan-green'"
                      :style="{ width: Math.min(100, u.dataUsagePercent || 0) + '%' }"
                    ></div>
                  </div>
                  <span v-if="u.dataLimitExceeded" class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-coral">
                    تجاوز الحصة
                  </span>
                </div>
              </td>
              <!-- Plan -->
              <td class="px-4 py-3 whitespace-nowrap">
                <span v-if="u.planName" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-golden-sand/20 text-golden-sand-dark">
                  {{ u.planName }}
                </span>
                <span v-else class="text-xs text-light-gray">—</span>
              </td>
              <!-- Status -->
              <td class="px-4 py-3 whitespace-nowrap">
                <span v-if="u.isSuspended" class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-coral">
                  موقوف - تجاوز الحصة
                </span>
                <span v-else-if="u.subscriptionStatus" class="text-xs text-light-gray">
                  {{ u.subscriptionStatus }}
                </span>
                <span v-else class="text-xs text-light-gray">—</span>
              </td>
              <!-- Actions -->
              <td class="px-4 py-3 whitespace-nowrap">
                <div class="flex items-center gap-1">
                  <button class="p-1.5 text-coastal-blue hover:bg-coastal-blue/10 rounded-lg transition-colors" title="تعديل" @click="openEditModal(u)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                  </button>
                  <button v-if="u.isOnline" class="p-1.5 text-golden-sand-dark hover:bg-golden-sand/10 rounded-lg transition-colors" title="قطع الاتصال" @click="confirmDisconnect(u.name)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
                    </svg>
                  </button>
                  <button v-if="u.limitBytesTotal > 0" class="p-1.5 text-teal hover:bg-teal/10 rounded-lg transition-colors" title="إعادة تعيين الحصة" @click="confirmResetQuota(u.name)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                    </svg>
                  </button>
                  <button class="p-1.5 hover:bg-gray-100 rounded-lg transition-colors" :class="u.disabled ? 'text-jazan-green' : 'text-golden-sand-dark'" :title="u.disabled ? 'تفعيل' : 'تعطيل'" @click="toggleUser(u.name, u.disabled)">
                    <svg v-if="u.disabled" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 9v6m4-6v6m7-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </button>
                  <button class="p-1.5 text-red-coral hover:bg-red-100 rounded-lg transition-colors" title="حذف" @click="confirmDelete(u.name)">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- No data after loading -->
        <div v-else-if="hasLoaded && !isLoading" class="text-center py-16">
          <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
          </svg>
          <p class="text-charcoal font-medium">{{ searchQuery ? 'لا توجد نتائج' : 'لا توجد بيانات' }}</p>
          <p v-if="errorMessage" class="text-red-coral text-sm mt-1">{{ errorMessage }}</p>
          <p v-else class="text-light-gray text-sm mt-1">{{ searchQuery ? 'جرب البحث بكلمات مختلفة' : 'لا يوجد مستخدمون PPP في جهاز MikroTik' }}</p>
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <div v-if="showEditModal && editingUser" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-xl shadow-xl max-w-lg w-full p-6">
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-lg font-semibold text-coastal-blue">تعديل المستخدم: {{ editingUser.name }}</h3>
          <button class="text-light-gray hover:text-charcoal" @click="showEditModal = false">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">كلمة المرور الجديدة</label>
            <input v-model="editForm.newPassword" type="password" placeholder="اتركه فارغاً للإبقاء على الحالية" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">البروفايل (السرعة)</label>
            <input v-model="editForm.newProfile" placeholder="default" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">حصة البيانات (GB)</label>
            <input v-model.number="editForm.limitGB" type="number" min="0" placeholder="0 = غير محدود" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
            <p class="text-xs text-light-gray mt-1">أدخل 0 لإلغاء الحد</p>
          </div>
          <div>
            <label class="block text-sm font-medium text-charcoal mb-1">ملاحظات</label>
            <input v-model="editForm.comment" placeholder="ملاحظات اختيارية" class="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
          </div>
        </div>
        <div class="flex justify-end gap-3 mt-6">
          <button class="px-4 py-2 text-light-gray hover:text-charcoal text-sm" @click="showEditModal = false">إلغاء</button>
          <button class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:opacity-90 text-sm" @click="saveEdit">حفظ التغييرات</button>
        </div>
      </div>
    </div>

    <!-- Confirmation Dialog -->
    <ConfirmDialog
      :visible="confirmVisible"
      :title="getConfirmTitle()"
      :message="getConfirmMessage()"
      :confirm-text="confirmAction === 'delete' ? 'حذف' : 'تأكيد'"
      cancel-text="إلغاء"
      @confirm="handleConfirm"
      @cancel="confirmVisible = false"
    />
  </div>
</template>
