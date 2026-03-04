<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToastStore } from '@/stores/toast'
import { apiGet } from '@/services/http'
import AppLoader from '@/components/AppLoader.vue'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()

const isLoading = ref(true)
const paymentStatus = ref<'success' | 'failed' | 'pending'>('pending')
const paymentDetails = ref<{
  orderId?: string
  transactionId?: string
  amount?: number
  currency?: string
  cardBrand?: string
  cardLast4?: string
  error?: string
} | null>(null)

async function verifyPayment() {
  const orderId = route.query.orderId as string || route.query.order as string
  
  if (!orderId) {
    paymentStatus.value = 'pending'
    isLoading.value = false
    return
  }

  try {
    const response = await apiGet<{
      success: boolean
      orderId: string
      transactionId: string
      status: string
      amount: number
      currency: string
      cardBrand: string
      cardLast4: string
      error?: string
    }>(`/payments/verify/${orderId}`)

    if (response.data?.success) {
      paymentStatus.value = 'success'
      paymentDetails.value = response.data
      toast.success('تم الدفع بنجاح! سيتم تفعيل اشتراكك خلال لحظات.')
    } else {
      paymentStatus.value = 'failed'
      paymentDetails.value = { error: response.data?.error || 'فشل التحقق من الدفع' }
    }
  } catch {
    paymentStatus.value = 'failed'
    paymentDetails.value = { error: 'حدث خطأ أثناء التحقق من حالة الدفع' }
  } finally {
    isLoading.value = false
  }
}

function goToSubscription() {
  router.push('/client/subscription')
}

function goToPlans() {
  router.push('/client/plans')
}

onMounted(() => {
  verifyPayment()
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="max-w-lg mx-auto">
    <!-- Success State -->
    <div v-if="paymentStatus === 'success'" class="bg-white rounded-2xl shadow-lg p-8 text-center border-2 border-teal">
      <div class="w-20 h-20 bg-teal/10 rounded-full flex items-center justify-center mx-auto mb-6">
        <svg class="h-10 w-10 text-teal" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
        </svg>
      </div>
      <h1 class="text-2xl font-bold text-coastal-blue mb-2">تم الدفع بنجاح!</h1>
      <p class="text-light-gray mb-6">شكراً لك. تم استلام دفعتك وسيتم تفعيل اشتراكك تلقائياً.</p>
      
      <div v-if="paymentDetails" class="bg-soft-beige-light rounded-xl p-4 mb-6 text-right">
        <div class="space-y-2 text-sm">
          <div v-if="paymentDetails.orderId" class="flex justify-between">
            <span class="text-light-gray">رقم الطلب:</span>
            <span class="font-mono text-charcoal">{{ paymentDetails.orderId }}</span>
          </div>
          <div v-if="paymentDetails.amount" class="flex justify-between">
            <span class="text-light-gray">المبلغ:</span>
            <span class="font-bold text-jazan-green">{{ paymentDetails.amount }} {{ paymentDetails.currency }}</span>
          </div>
          <div v-if="paymentDetails.cardBrand" class="flex justify-between">
            <span class="text-light-gray">البطاقة:</span>
            <span class="text-charcoal">{{ paymentDetails.cardBrand }} **** {{ paymentDetails.cardLast4 }}</span>
          </div>
        </div>
      </div>

      <button 
        class="w-full py-3 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors font-medium"
        @click="goToSubscription"
      >
        عرض اشتراكي
      </button>
    </div>

    <!-- Failed State -->
    <div v-else-if="paymentStatus === 'failed'" class="bg-white rounded-2xl shadow-lg p-8 text-center border-2 border-red-coral">
      <div class="w-20 h-20 bg-red-coral/10 rounded-full flex items-center justify-center mx-auto mb-6">
        <svg class="h-10 w-10 text-red-coral" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </div>
      <h1 class="text-2xl font-bold text-coastal-blue mb-2">فشل الدفع</h1>
      <p class="text-light-gray mb-6">{{ paymentDetails?.error || 'لم يتم إتمام عملية الدفع. الرجاء المحاولة مرة أخرى.' }}</p>
      
      <button 
        class="w-full py-3 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors font-medium"
        @click="goToPlans"
      >
        العودة للباقات
      </button>
    </div>

    <!-- Pending State -->
    <div v-else class="bg-white rounded-2xl shadow-lg p-8 text-center border-2 border-warning-yellow">
      <div class="w-20 h-20 bg-warning-yellow/10 rounded-full flex items-center justify-center mx-auto mb-6">
        <svg class="h-10 w-10 text-warning-yellow animate-spin" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
      </div>
      <h1 class="text-2xl font-bold text-coastal-blue mb-2">جاري التحقق...</h1>
      <p class="text-light-gray mb-6">يتم التحقق من حالة الدفع. الرجاء الانتظار.</p>
    </div>
  </div>
</template>
