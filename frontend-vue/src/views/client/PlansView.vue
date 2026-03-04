<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePlansStore } from '@/stores/plans'
import { useSubscriptionsStore } from '@/stores/subscriptions'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import { apiGet, apiPost } from '@/services/http'
import AppLoader from '@/components/AppLoader.vue'
import type { Plan, Subscriber } from '@/types'

const plansStore = usePlansStore()
const subscriptionsStore = useSubscriptionsStore()
const toast = useToastStore()
const { formatCurrency } = useFormatters()

const selectedPlan = ref<Plan | null>(null)
const showPaymentInfo = ref(false)
const selectedMethod = ref('Card')
const myProfile = ref<Subscriber | null>(null)
const hasActiveSubscription = ref(false)
const isProcessing = ref(false)

const paymentChannels = [
  { value: 'Card', label: 'بطاقة (مدى/Visa/MC)', icon: '💳', desc: 'ادفع عبر بطاقة مدى أو Visa أو MasterCard' },
  { value: 'BankTransfer', label: 'تحويل بنكي', icon: '🏦', desc: 'حوّل المبلغ إلى حساب الشركة البنكي' },
]

function selectPlan(plan: Plan) {
  if (hasActiveSubscription.value) {
    toast.error('لديك اشتراك نشط بالفعل. يرجى الانتظار حتى انتهاء اشتراكك الحالي.')
    return
  }
  selectedPlan.value = plan
  showPaymentInfo.value = true
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function cancelSelection() {
  selectedPlan.value = null
  showPaymentInfo.value = false
}

async function confirmPurchase() {
  if (!selectedPlan.value || !myProfile.value) return
  
  isProcessing.value = true
  
  try {
    const response = await apiPost<{
      success: boolean
      subscriptionId: string
      transactionId: string
      redirectUrl: string | null
      error?: string
    }>('/payments/create-session', {
      planId: selectedPlan.value.id,
      subscriberId: myProfile.value.id,
      paymentMethod: selectedMethod.value
    })

    if (!response.success || response.data?.error) {
      toast.error(response.data?.error || 'فشل في إنشاء جلسة الدفع')
      return
    }

    if (selectedMethod.value === 'BankTransfer') {
      toast.success('تم إرسال طلب الاشتراك بنجاح! سيتم تفعيل اشتراكك بعد تأكيد الدفع من قبل المسؤول.')
      showPaymentInfo.value = false
      selectedPlan.value = null
      return
    }

    if (response.data?.redirectUrl) {
      toast.info('جاري توجيهك لبوابة الدفع...')
      window.location.href = response.data.redirectUrl
    } else {
      toast.error('لم يتم استلام رابط الدفع. الرجاء المحاولة مرة أخرى.')
    }
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء جلسة الدفع'
    toast.error(message)
  } finally {
    isProcessing.value = false
  }
}

onMounted(async () => {
  await plansStore.fetchPlans(true)
  
  try {
    const [profileRes] = await Promise.all([
      apiGet<Subscriber>('/subscribers/me'),
      subscriptionsStore.fetchAll()
    ])
    if (profileRes.data) {
      myProfile.value = profileRes.data
    }
    hasActiveSubscription.value = subscriptionsStore.subscriptions.some(
      s => s.status === 'Active' || s.status === 'PendingActivation'
    )
  } catch {
    // Ignore errors - user might not have profile yet
  }
})
</script>

<template>
  <AppLoader :visible="plansStore.isLoading" />
  <div>
    <h1 class="text-2xl font-bold text-coastal-blue mb-2">الباقات المتاحة</h1>
    <p class="text-light-gray mb-6">اختر الباقة المناسبة لك وقم بالدفع عبر إحدى القنوات المتاحة</p>

    <!-- Payment Info Panel -->
    <div v-if="showPaymentInfo && selectedPlan" class="bg-white rounded-xl shadow-lg p-6 mb-8 border-2 border-jazan-green">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-jazan-green">إتمام الاشتراك</h2>
        <button class="text-light-gray hover:text-charcoal" @click="cancelSelection">
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Selected Plan Summary -->
      <div class="bg-jazan-green/5 rounded-lg p-4 mb-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="font-semibold text-charcoal text-lg">{{ selectedPlan.name }}</p>
            <p class="text-sm text-light-gray">{{ selectedPlan.speedMbps }} Mbps - {{ selectedPlan.billingCycleDays }} يوم</p>
          </div>
          <p class="text-2xl font-bold text-jazan-green">{{ formatCurrency(selectedPlan.price) }}</p>
        </div>
      </div>

      <!-- Payment Method Selection -->
      <h3 class="text-sm font-semibold text-charcoal mb-3">اختر طريقة الدفع</h3>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 mb-6">
        <button
          v-for="ch in paymentChannels"
          :key="ch.value"
          :class="[
            'flex items-center gap-3 p-4 rounded-xl border-2 text-right transition-all',
            selectedMethod === ch.value
              ? 'border-coastal-blue bg-coastal-blue/5'
              : 'border-gray-200 hover:border-coastal-blue/50'
          ]"
          @click="selectedMethod = ch.value"
        >
          <div :class="['h-10 w-10 rounded-lg flex items-center justify-center text-white font-bold text-sm shrink-0', selectedMethod === ch.value ? 'bg-coastal-blue' : 'bg-gray-300']">
            {{ ch.icon }}
          </div>
          <div>
            <p class="font-medium text-charcoal">{{ ch.label }}</p>
            <p class="text-xs text-light-gray">{{ ch.desc }}</p>
          </div>
        </button>
      </div>

      <!-- Bank Transfer Info -->
      <div v-if="selectedMethod === 'BankTransfer'" class="bg-soft-beige-light rounded-xl p-4 mb-6">
        <h4 class="text-sm font-semibold text-coastal-blue mb-3">معلومات التحويل البنكي</h4>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between"><span class="text-light-gray">اسم البنك:</span><span class="font-medium text-charcoal">بنك الراجحي</span></div>
          <div class="flex justify-between"><span class="text-light-gray">رقم الحساب:</span><span class="font-medium text-charcoal font-mono">SA0000000000000000000000</span></div>
          <div class="flex justify-between"><span class="text-light-gray">اسم المستفيد:</span><span class="font-medium text-charcoal">شركة Dushi للاتصالات</span></div>
          <div class="flex justify-between"><span class="text-light-gray">المبلغ المطلوب:</span><span class="font-bold text-jazan-green">{{ formatCurrency(selectedPlan.price) }}</span></div>
        </div>
        <div class="mt-3 bg-warning-yellow/10 border border-warning-yellow/30 rounded-lg p-3">
          <p class="text-xs text-golden-sand-dark"><strong>مهم:</strong> أرسل إيصال التحويل عبر واتساب أو البريد الإلكتروني. سيتم تفعيل اشتراكك خلال 24 ساعة بعد التحقق.</p>
        </div>
      </div>

      <!-- Card Payment Info -->
      <div v-else class="bg-soft-beige-light rounded-xl p-4 mb-6">
        <h4 class="text-sm font-semibold text-coastal-blue mb-3">الدفع عبر البطاقة</h4>
        <div class="text-center py-4">
          <div class="flex justify-center gap-4 mb-4">
            <div class="bg-white rounded-lg p-2 shadow-sm">
              <span class="text-2xl font-bold text-green-600">مدى</span>
            </div>
            <div class="bg-white rounded-lg p-2 shadow-sm">
              <span class="text-2xl font-bold text-blue-600">VISA</span>
            </div>
            <div class="bg-white rounded-lg p-2 shadow-sm">
              <span class="text-2xl font-bold text-orange-600">MC</span>
            </div>
          </div>
          <p class="text-sm text-light-gray">سيتم توجيهك لبوابة الدفع الآمنة (Geidea) لإتمام العملية</p>
          <p class="text-xs text-light-gray mt-1">المبلغ: <strong class="text-jazan-green">{{ formatCurrency(selectedPlan.price) }}</strong></p>
          <div class="mt-3 flex items-center justify-center gap-2 text-xs text-teal">
            <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
            <span>دفع آمن ومشفر - 3D Secure</span>
          </div>
        </div>
      </div>

      <!-- Confirm Button -->
      <div class="flex items-center justify-between">
        <button 
          class="px-6 py-2 text-sm text-light-gray hover:text-charcoal transition-colors disabled:opacity-50" 
          :disabled="isProcessing"
          @click="cancelSelection"
        >
          إلغاء
        </button>
        <button 
          class="px-8 py-3 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors shadow-md font-medium disabled:opacity-50 flex items-center gap-2" 
          :disabled="isProcessing"
          @click="confirmPurchase"
        >
          <svg v-if="isProcessing" class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ isProcessing ? 'جاري المعالجة...' : (selectedMethod === 'BankTransfer' ? 'تأكيد طلب الاشتراك' : 'ادفع الآن') }}
        </button>
      </div>
    </div>

    <!-- Plans Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="plan in plansStore.plans"
        :key="plan.id"
        :class="[
          'bg-white rounded-xl shadow-md border-2 overflow-hidden transition-all hover:shadow-lg',
          selectedPlan?.id === plan.id ? 'border-jazan-green' : 'border-soft-beige'
        ]"
      >
        <div class="p-6">
          <h3 class="text-lg font-bold text-coastal-blue mb-1">{{ plan.name }}</h3>
          <p class="text-sm text-light-gray mb-4">{{ plan.description }}</p>

          <div class="text-center py-4">
            <p class="text-3xl font-bold text-jazan-green">{{ formatCurrency(plan.price) }}</p>
            <p class="text-xs text-light-gray mt-1">/ {{ plan.billingCycleDays }} يوم</p>
          </div>

          <div class="space-y-3 text-sm mb-6">
            <div class="flex items-center gap-2">
              <svg class="h-4 w-4 text-teal shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>
              <span class="text-charcoal">سرعة <strong>{{ plan.speedMbps }} Mbps</strong></span>
            </div>
            <div class="flex items-center gap-2">
              <svg class="h-4 w-4 text-teal shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>
              <span class="text-charcoal">{{ plan.dataLimitGB === 0 ? 'بيانات غير محدودة' : plan.dataLimitGB + ' GB' }}</span>
            </div>
            <div class="flex items-center gap-2">
              <svg class="h-4 w-4 text-teal shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>
              <span class="text-charcoal">دورة فوترة {{ plan.billingCycleDays }} يوم</span>
            </div>
          </div>

          <button
            class="w-full py-3 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors font-medium shadow-md"
            @click="selectPlan(plan)"
          >
            اشترك الآن
          </button>
        </div>
      </div>
    </div>

    <div v-if="!plansStore.plans.length && !plansStore.isLoading" class="text-center py-16 text-light-gray">
      <svg class="mx-auto h-12 w-12 text-pale-olive mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
      </svg>
      <p>لا توجد باقات متاحة حالياً</p>
    </div>
  </div>
</template>
