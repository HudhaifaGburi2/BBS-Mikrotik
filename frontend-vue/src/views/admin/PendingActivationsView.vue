<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { apiGet, apiPost } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import AppLoader from '@/components/AppLoader.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'

interface PendingActivation {
  subscriptionId: string
  subscriberId: string
  subscriberName: string
  subscriberEmail: string
  subscriberPhone: string
  planId: string
  planName: string
  planPrice: number
  dataLimitGB: number
  speedMbps: number
  paymentStatus: string
  requestDate: string
  paidAt: string | null
  pppUsername: string | null
}

const toast = useToastStore()
const { formatDate, formatCurrency } = useFormatters()

const pendingActivations = ref<PendingActivation[]>([])
const isLoading = ref(false)
const searchQuery = ref('')
const filterPayment = ref<'all' | 'paid' | 'unpaid'>('all')

// Confirm dialogs
const confirmVisible = ref(false)
const confirmAction = ref<'activate' | 'reject'>('activate')
const selectedSubscription = ref<PendingActivation | null>(null)
const rejectReason = ref('')
const showRejectModal = ref(false)

const filteredActivations = computed(() => {
  let result = pendingActivations.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(a =>
      a.subscriberName.toLowerCase().includes(query) ||
      a.subscriberEmail.toLowerCase().includes(query) ||
      a.planName.toLowerCase().includes(query) ||
      a.pppUsername?.toLowerCase().includes(query)
    )
  }

  if (filterPayment.value === 'paid') {
    result = result.filter(a => a.paidAt !== null)
  } else if (filterPayment.value === 'unpaid') {
    result = result.filter(a => a.paidAt === null)
  }

  return result
})

const stats = computed(() => ({
  total: pendingActivations.value.length,
  paid: pendingActivations.value.filter(a => a.paidAt !== null).length,
  unpaid: pendingActivations.value.filter(a => a.paidAt === null).length
}))

async function loadPendingActivations() {
  isLoading.value = true
  try {
    const res = await apiGet<{ success: boolean; data: PendingActivation[] }>('/subscriptions/pending-activation')
    if (res.data?.success && res.data.data) {
      pendingActivations.value = res.data.data
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تحميل طلبات التفعيل')
  } finally {
    isLoading.value = false
  }
}

function confirmActivate(activation: PendingActivation) {
  selectedSubscription.value = activation
  confirmAction.value = 'activate'
  confirmVisible.value = true
}

function openRejectModal(activation: PendingActivation) {
  selectedSubscription.value = activation
  rejectReason.value = ''
  showRejectModal.value = true
}

async function handleActivate() {
  if (!selectedSubscription.value) return

  try {
    const res = await apiPost<{ success: boolean; message?: string }>(`/subscriptions/${selectedSubscription.value.subscriptionId}/activate`, {})
    if (res.data?.success) {
      toast.success('تم تفعيل الاشتراك بنجاح')
      await loadPendingActivations()
    } else {
      toast.error(res.data?.message || 'فشل تفعيل الاشتراك')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل تفعيل الاشتراك')
  } finally {
    confirmVisible.value = false
    selectedSubscription.value = null
  }
}

async function handleReject() {
  if (!selectedSubscription.value) return

  try {
    const res = await apiPost<{ success: boolean; message?: string }>(`/subscriptions/${selectedSubscription.value.subscriptionId}/reject`, {
      reason: rejectReason.value || 'تم رفض الطلب من قبل الإدارة'
    })
    if (res.data?.success) {
      toast.success('تم رفض طلب التفعيل')
      await loadPendingActivations()
    } else {
      toast.error(res.data?.message || 'فشل رفض الطلب')
    }
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'فشل رفض الطلب')
  } finally {
    showRejectModal.value = false
    selectedSubscription.value = null
    rejectReason.value = ''
  }
}

onMounted(() => {
  loadPendingActivations()
})
</script>

<template>
  <AppLoader :visible="isLoading" />

  <div>
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-coastal-blue">إدارة طلبات التفعيل</h1>
        <p class="text-light-gray text-sm mt-1">قائمة الاشتراكات المعلقة في انتظار التفعيل</p>
      </div>
      <button
        @click="loadPendingActivations"
        class="px-4 py-2 bg-coastal-blue text-white rounded-lg hover:bg-coastal-blue/90 transition-colors flex items-center gap-2"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
        تحديث
      </button>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
      <div class="bg-white rounded-xl shadow-md p-4 border border-soft-beige">
        <div class="flex items-center gap-3">
          <div class="w-12 h-12 rounded-full bg-golden-sand/20 flex items-center justify-center">
            <svg class="w-6 h-6 text-golden-sand-dark" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-charcoal">{{ stats.total }}</p>
            <p class="text-sm text-light-gray">إجمالي الطلبات المعلقة</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-md p-4 border border-soft-beige">
        <div class="flex items-center gap-3">
          <div class="w-12 h-12 rounded-full bg-jazan-green/20 flex items-center justify-center">
            <svg class="w-6 h-6 text-jazan-green" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-jazan-green">{{ stats.paid }}</p>
            <p class="text-sm text-light-gray">مدفوع - جاهز للتفعيل</p>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-md p-4 border border-soft-beige">
        <div class="flex items-center gap-3">
          <div class="w-12 h-12 rounded-full bg-warning-yellow/20 flex items-center justify-center">
            <svg class="w-6 h-6 text-warning-yellow" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-2xl font-bold text-warning-yellow">{{ stats.unpaid }}</p>
            <p class="text-sm text-light-gray">في انتظار الدفع</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white rounded-xl shadow-md p-4 mb-6 border border-soft-beige">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="بحث بالاسم، البريد، الباقة..."
            class="w-full px-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-coastal-blue/20 focus:border-coastal-blue"
          />
        </div>
        <div class="flex gap-2">
          <button
            @click="filterPayment = 'all'"
            :class="filterPayment === 'all' ? 'bg-coastal-blue text-white' : 'bg-gray-100 text-charcoal'"
            class="px-4 py-2 rounded-lg transition-colors"
          >
            الكل
          </button>
          <button
            @click="filterPayment = 'paid'"
            :class="filterPayment === 'paid' ? 'bg-jazan-green text-white' : 'bg-gray-100 text-charcoal'"
            class="px-4 py-2 rounded-lg transition-colors"
          >
            مدفوع
          </button>
          <button
            @click="filterPayment = 'unpaid'"
            :class="filterPayment === 'unpaid' ? 'bg-warning-yellow text-white' : 'bg-gray-100 text-charcoal'"
            class="px-4 py-2 rounded-lg transition-colors"
          >
            غير مدفوع
          </button>
        </div>
      </div>
    </div>

    <!-- Pending Activations List -->
    <div class="bg-white rounded-xl shadow-md border border-soft-beige overflow-hidden">
      <div v-if="filteredActivations.length === 0" class="p-12 text-center">
        <svg class="w-16 h-16 mx-auto text-light-gray mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-charcoal font-medium text-lg">لا توجد طلبات تفعيل معلقة</p>
        <p class="text-light-gray text-sm mt-1">جميع الاشتراكات مفعلة</p>
      </div>

      <div v-else class="divide-y divide-gray-200">
        <div
          v-for="activation in filteredActivations"
          :key="activation.subscriptionId"
          class="p-4 hover:bg-gray-50 transition-colors"
        >
          <div class="flex flex-col lg:flex-row lg:items-center gap-4">
            <!-- Subscriber Info -->
            <div class="flex-1">
              <div class="flex items-center gap-3 mb-2">
                <div class="w-10 h-10 rounded-full bg-coastal-blue/10 flex items-center justify-center">
                  <svg class="w-5 h-5 text-coastal-blue" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                </div>
                <div>
                  <p class="font-semibold text-charcoal">{{ activation.subscriberName }}</p>
                  <p class="text-xs text-light-gray">{{ activation.subscriberEmail }}</p>
                </div>
              </div>
              <div class="flex flex-wrap gap-4 text-sm">
                <div>
                  <span class="text-light-gray">الهاتف:</span>
                  <span class="font-medium text-charcoal mr-1">{{ activation.subscriberPhone }}</span>
                </div>
                <div v-if="activation.pppUsername">
                  <span class="text-light-gray">PPP:</span>
                  <span class="font-mono font-medium text-charcoal mr-1">{{ activation.pppUsername }}</span>
                </div>
              </div>
            </div>

            <!-- Plan Info -->
            <div class="lg:w-1/4">
              <div class="bg-golden-sand/10 rounded-lg p-3">
                <p class="font-semibold text-golden-sand-dark">{{ activation.planName }}</p>
                <div class="flex flex-wrap gap-2 mt-1 text-xs">
                  <span class="bg-white px-2 py-0.5 rounded">{{ activation.speedMbps }} Mbps</span>
                  <span class="bg-white px-2 py-0.5 rounded">
                    {{ activation.dataLimitGB > 0 ? activation.dataLimitGB + ' GB' : 'غير محدود' }}
                  </span>
                </div>
                <p class="text-sm font-bold text-charcoal mt-2">{{ formatCurrency(activation.planPrice) }}</p>
              </div>
            </div>

            <!-- Payment Status -->
            <div class="lg:w-1/6 text-center">
              <span
                :class="activation.paidAt ? 'bg-green-100 text-jazan-green' : 'bg-yellow-100 text-warning-yellow'"
                class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium"
              >
                {{ activation.paymentStatus }}
              </span>
              <p class="text-xs text-light-gray mt-1">
                {{ formatDate(activation.requestDate) }}
              </p>
            </div>

            <!-- Actions -->
            <div class="flex gap-2 lg:w-auto">
              <button
                @click="confirmActivate(activation)"
                class="px-4 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green/90 transition-colors flex items-center gap-1"
                :class="{ 'opacity-50 cursor-not-allowed': !activation.paidAt }"
                :disabled="!activation.paidAt"
                :title="!activation.paidAt ? 'يجب الدفع أولاً' : 'تفعيل الاشتراك'"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                </svg>
                تفعيل
              </button>
              <button
                @click="openRejectModal(activation)"
                class="px-4 py-2 bg-red-coral/10 text-red-coral rounded-lg hover:bg-red-coral/20 transition-colors flex items-center gap-1"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
                رفض
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Activate Confirmation Dialog -->
  <ConfirmDialog
    :visible="confirmVisible"
    title="تأكيد التفعيل"
    :message="`هل أنت متأكد من تفعيل اشتراك ${selectedSubscription?.subscriberName}؟ سيتم تفعيل حساب PPP على MikroTik.`"
    confirmText="تفعيل"
    confirmClass="bg-jazan-green hover:bg-jazan-green/90"
    @confirm="handleActivate"
    @cancel="confirmVisible = false"
  />

  <!-- Reject Modal -->
  <Teleport to="body">
    <div v-if="showRejectModal" class="fixed inset-0 z-50 flex items-center justify-center">
      <div class="absolute inset-0 bg-black/50" @click="showRejectModal = false"></div>
      <div class="relative bg-white rounded-xl shadow-xl p-6 w-full max-w-md mx-4">
        <h3 class="text-lg font-bold text-charcoal mb-4">رفض طلب التفعيل</h3>
        <p class="text-sm text-light-gray mb-4">
          سيتم رفض طلب تفعيل اشتراك <strong>{{ selectedSubscription?.subscriberName }}</strong>
        </p>
        <div class="mb-4">
          <label class="block text-sm font-medium text-charcoal mb-1">سبب الرفض</label>
          <textarea
            v-model="rejectReason"
            rows="3"
            class="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-red-coral/20 focus:border-red-coral"
            placeholder="أدخل سبب الرفض (اختياري)"
          ></textarea>
        </div>
        <div class="flex gap-3 justify-end">
          <button
            @click="showRejectModal = false"
            class="px-4 py-2 bg-gray-100 text-charcoal rounded-lg hover:bg-gray-200 transition-colors"
          >
            إلغاء
          </button>
          <button
            @click="handleReject"
            class="px-4 py-2 bg-red-coral text-white rounded-lg hover:bg-red-coral/90 transition-colors"
          >
            رفض الطلب
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
