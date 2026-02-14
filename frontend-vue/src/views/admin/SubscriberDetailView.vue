<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSubscribersStore } from '@/stores/subscribers'
import { usePlansStore } from '@/stores/plans'
import { useToastStore } from '@/stores/toast'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

const route = useRoute()
const router = useRouter()
const store = useSubscribersStore()
const plansStore = usePlansStore()
const toast = useToastStore()

const id = computed(() => route.params.id as string)
const sub = computed(() => store.currentSubscriber)
const isLoading = ref(false)

// Password forms
const systemPassword = ref('')
const mikroTikPassword = ref('')
const showSystemPwForm = ref(false)
const showMikroTikPwForm = ref(false)

// Plan change
const showChangePlan = ref(false)
const selectedPlanId = ref('')

onMounted(async () => {
  isLoading.value = true
  try {
    await Promise.all([
      store.fetchSubscriber(id.value),
      plansStore.fetchPlans(true),
    ])
  } finally {
    isLoading.value = false
  }
})

async function handleResetSystemPassword() {
  if (!systemPassword.value || systemPassword.value.length < 8) {
    toast.error('كلمة المرور يجب أن تكون 8 أحرف على الأقل')
    return
  }
  try {
    await store.resetSystemPassword(id.value, systemPassword.value)
    toast.success('تم تغيير كلمة مرور النظام بنجاح')
    systemPassword.value = ''
    showSystemPwForm.value = false
  } catch {
    toast.error('فشل تغيير كلمة مرور النظام — قد لا يكون لدى المشترك حساب نظام')
  }
}

async function handleResetMikroTikPassword() {
  if (!mikroTikPassword.value) {
    toast.error('الرجاء إدخال كلمة المرور الجديدة')
    return
  }
  try {
    await store.resetMikroTikPassword(id.value, mikroTikPassword.value)
    toast.success('تم تغيير كلمة مرور MikroTik بنجاح')
    mikroTikPassword.value = ''
    showMikroTikPwForm.value = false
  } catch {
    toast.error('فشل تغيير كلمة مرور MikroTik — قد لا يكون لدى المشترك حساب PPPoE')
  }
}

async function handleSuspend() {
  try {
    await store.suspendSubscriber(id.value)
    toast.success('تم إيقاف المشترك')
    await store.fetchSubscriber(id.value)
  } catch { toast.error('فشل إيقاف المشترك') }
}

async function handleUnsuspend() {
  try {
    await store.unsuspendSubscriber(id.value)
    toast.success('تم تفعيل المشترك')
    await store.fetchSubscriber(id.value)
  } catch { toast.error('فشل تفعيل المشترك') }
}

async function handleChangePlan() {
  if (!selectedPlanId.value) {
    toast.error('الرجاء اختيار باقة')
    return
  }
  try {
    await store.changePlan(id.value, selectedPlanId.value)
    toast.success('تم تغيير الباقة بنجاح')
    showChangePlan.value = false
    selectedPlanId.value = ''
    await store.fetchSubscriber(id.value)
  } catch { toast.error('فشل تغيير الباقة') }
}

async function handleDelete() {
  if (!confirm('هل أنت متأكد من حذف هذا المشترك؟')) return
  try {
    await store.deleteSubscriber(id.value, true)
    toast.success('تم حذف المشترك')
    router.push('/admin/subscribers')
  } catch { toast.error('فشل حذف المشترك') }
}
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div v-if="sub" class="max-w-4xl mx-auto space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-coastal-blue">{{ sub.fullName }}</h1>
        <p class="text-sm text-light-gray">{{ sub.email }} · {{ sub.phoneNumber }}</p>
      </div>
      <div class="flex items-center gap-3">
        <StatusBadge :status="sub.isActive ? 'Active' : 'Suspended'" />
        <button class="px-4 py-2 text-sm bg-coastal-blue text-white rounded-lg hover:bg-coastal-blue-dark transition-colors" @click="router.push(`/admin/subscribers/${id}/edit`)">تعديل</button>
        <RouterLink to="/admin/subscribers" class="px-4 py-2 text-sm bg-pale-olive/30 text-charcoal rounded-lg hover:bg-pale-olive/50 transition-colors">رجوع</RouterLink>
      </div>
    </div>

    <!-- Info Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Personal Info -->
      <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <h3 class="text-lg font-semibold text-coastal-blue mb-4">المعلومات الشخصية</h3>
        <div class="space-y-3 text-sm">
          <div class="flex justify-between"><span class="text-light-gray">الاسم:</span><span class="font-medium text-charcoal">{{ sub.fullName }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">البريد:</span><span class="font-medium text-charcoal">{{ sub.email }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">الهاتف:</span><span class="font-medium text-charcoal">{{ sub.phoneNumber }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">الهوية:</span><span class="font-medium text-charcoal">{{ sub.nationalId || '—' }}</span></div>
          <div class="flex justify-between"><span class="text-light-gray">العنوان:</span><span class="font-medium text-charcoal">{{ sub.address }}</span></div>
        </div>
      </div>

      <!-- Subscription Info -->
      <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold text-jazan-green">الاشتراك</h3>
          <button class="text-xs text-coastal-blue hover:underline" @click="showChangePlan = !showChangePlan">تغيير الباقة</button>
        </div>
        <div v-if="sub.subscriptions?.length" class="space-y-3 text-sm">
          <div v-for="s in sub.subscriptions" :key="s.id" class="bg-jazan-green/5 rounded-lg p-3">
            <div class="flex justify-between mb-1"><span class="font-semibold text-charcoal">{{ s.planName }}</span><StatusBadge :status="String(s.status) === '1' ? 'Active' : 'Pending'" /></div>
            <div class="text-xs text-light-gray">{{ new Date(s.startDate).toLocaleDateString('ar-SA') }} → {{ new Date(s.endDate).toLocaleDateString('ar-SA') }}</div>
          </div>
        </div>
        <p v-else class="text-sm text-light-gray">لا يوجد اشتراك</p>

        <!-- Change Plan Form -->
        <div v-if="showChangePlan" class="mt-4 pt-4 border-t border-gray-100">
          <select v-model="selectedPlanId" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm mb-2">
            <option value="">اختر باقة...</option>
            <option v-for="plan in plansStore.plans" :key="plan.id" :value="plan.id">{{ plan.name }} - {{ plan.price }} {{ plan.currency }}</option>
          </select>
          <button class="w-full px-4 py-2 bg-jazan-green text-white rounded-lg text-sm hover:bg-jazan-green-dark transition-colors" @click="handleChangePlan">تغيير</button>
        </div>
      </div>
    </div>

    <!-- PPPoE Accounts -->
    <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
      <h3 class="text-lg font-semibold text-coastal-blue mb-4">حسابات MikroTik (PPPoE)</h3>
      <div v-if="sub.pppoeAccounts?.length" class="space-y-3">
        <div v-for="ppp in sub.pppoeAccounts" :key="ppp.id" class="bg-soft-beige-light rounded-lg p-4">
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 text-sm">
            <div><p class="text-light-gray text-xs">اسم المستخدم</p><p class="font-mono font-semibold text-charcoal">{{ ppp.username }}</p></div>
            <div><p class="text-light-gray text-xs">الباقة</p><p class="font-medium text-charcoal">{{ ppp.profileName }}</p></div>
            <div><p class="text-light-gray text-xs">الحالة</p><StatusBadge :status="ppp.isEnabled ? 'Active' : 'Disabled'" /></div>
            <div><p class="text-light-gray text-xs">المزامنة</p><StatusBadge :status="ppp.isSyncedWithMikroTik ? 'Synced' : 'NotSynced'" /></div>
          </div>
          <div v-if="ppp.lastConnectedAt" class="mt-2 text-xs text-light-gray">
            آخر اتصال: {{ new Date(ppp.lastConnectedAt).toLocaleString('ar-SA') }}
          </div>
        </div>
      </div>
      <p v-else class="text-sm text-light-gray">لا توجد حسابات PPPoE</p>
    </div>

    <!-- Password Management -->
    <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
      <h3 class="text-lg font-semibold text-red-coral mb-4">إدارة كلمات المرور</h3>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <!-- System Password -->
        <div class="border border-gray-200 rounded-xl p-4">
          <div class="flex items-center justify-between mb-3">
            <div>
              <h4 class="font-semibold text-charcoal text-sm">كلمة مرور النظام</h4>
              <p class="text-xs text-light-gray">لتسجيل الدخول إلى لوحة التحكم</p>
            </div>
            <button class="text-xs px-3 py-1 bg-coastal-blue text-white rounded-lg hover:bg-coastal-blue-dark transition-colors" @click="showSystemPwForm = !showSystemPwForm">
              {{ showSystemPwForm ? 'إلغاء' : 'تغيير' }}
            </button>
          </div>
          <div v-if="showSystemPwForm" class="space-y-2">
            <input v-model="systemPassword" type="password" placeholder="كلمة المرور الجديدة (8 أحرف على الأقل)" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" />
            <button class="w-full px-4 py-2 bg-coastal-blue text-white rounded-lg text-sm hover:bg-coastal-blue-dark transition-colors" @click="handleResetSystemPassword">حفظ كلمة مرور النظام</button>
          </div>
        </div>

        <!-- MikroTik Password -->
        <div class="border border-gray-200 rounded-xl p-4">
          <div class="flex items-center justify-between mb-3">
            <div>
              <h4 class="font-semibold text-charcoal text-sm">كلمة مرور MikroTik</h4>
              <p class="text-xs text-light-gray">لاتصال PPPoE بالراوتر</p>
            </div>
            <button class="text-xs px-3 py-1 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors" @click="showMikroTikPwForm = !showMikroTikPwForm">
              {{ showMikroTikPwForm ? 'إلغاء' : 'تغيير' }}
            </button>
          </div>
          <div v-if="showMikroTikPwForm" class="space-y-2">
            <input v-model="mikroTikPassword" type="password" placeholder="كلمة مرور MikroTik الجديدة" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" />
            <button class="w-full px-4 py-2 bg-jazan-green text-white rounded-lg text-sm hover:bg-jazan-green-dark transition-colors" @click="handleResetMikroTikPassword">حفظ كلمة مرور MikroTik</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Actions -->
    <div class="bg-white rounded-xl shadow-md p-6 border border-soft-beige">
      <h3 class="text-lg font-semibold text-charcoal mb-4">إجراءات</h3>
      <div class="flex flex-wrap gap-3">
        <button v-if="sub.isActive" class="px-4 py-2 text-sm bg-warning-yellow text-charcoal rounded-lg hover:bg-warning-yellow/80 transition-colors" @click="handleSuspend">
          إيقاف المشترك
        </button>
        <button v-else class="px-4 py-2 text-sm bg-teal text-white rounded-lg hover:bg-teal/80 transition-colors" @click="handleUnsuspend">
          تفعيل المشترك
        </button>
        <button class="px-4 py-2 text-sm bg-red-coral text-white rounded-lg hover:bg-red-coral/80 transition-colors" @click="handleDelete">
          حذف المشترك
        </button>
      </div>
    </div>
  </div>

  <div v-else-if="!isLoading" class="text-center py-16 text-light-gray">
    <p>المشترك غير موجود</p>
    <RouterLink to="/admin/subscribers" class="text-coastal-blue hover:underline mt-2 inline-block">العودة للقائمة</RouterLink>
  </div>
</template>
