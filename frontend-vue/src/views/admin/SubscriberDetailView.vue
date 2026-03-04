<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSubscribersStore } from '@/stores/subscribers'
import { usePlansStore } from '@/stores/plans'
import { useToastStore } from '@/stores/toast'
import { useFormatters } from '@/composables/useFormatters'
import { apiPost } from '@/services/http'
import AppLoader from '@/components/AppLoader.vue'
import StatusBadge from '@/components/StatusBadge.vue'

interface ActiveSession {
  id: string
  name: string
  address: string | null
  uptime: string | null
  bytesIn: number
  bytesOut: number
  limitBytesIn: number | null
  limitBytesOut: number | null
}

const route = useRoute()
const router = useRouter()
const store = useSubscribersStore()
const plansStore = usePlansStore()
const toast = useToastStore()
const { formatBytes } = useFormatters()

// Real-time session data from MikroTik
const activeSession = ref<ActiveSession | null>(null)
const isLoadingSession = ref(false)

function getDataUsagePercent(usedBytes: number, limitGB: number): number {
  if (!limitGB || limitGB <= 0) return 0
  const limitBytes = limitGB * 1024 * 1024 * 1024
  return Math.min(100, Math.round((usedBytes / limitBytes) * 100))
}

function getRemainingGB(usedBytes: number, limitGB: number): string {
  if (!limitGB || limitGB <= 0) return 'غير محدود'
  const usedGB = usedBytes / (1024 * 1024 * 1024)
  const remaining = Math.max(0, limitGB - usedGB)
  return remaining.toFixed(2) + ' GB'
}

const id = computed(() => route.params.id as string)
const sub = computed(() => store.currentSubscriber)
const isLoading = ref(false)

// Password forms
const systemPassword = ref('')
const mikroTikPassword = ref('')
const showSystemPwForm = ref(false)
const showMikroTikPwForm = ref(false)
const isResettingSystemPw = ref(false)
const isResettingMikroTikPw = ref(false)

// Plan change
const showChangePlan = ref(false)
const selectedPlanId = ref('')

async function fetchActiveSession() {
  const pppUsername = sub.value?.pppoeAccounts?.[0]?.username
  if (!pppUsername) return
  
  isLoadingSession.value = true
  try {
    const res = await apiPost<{ success: boolean; data: ActiveSession[] }>('/mikrotik/active-sessions', {})
    if (res.data?.success && res.data.data) {
      activeSession.value = res.data.data.find(s => s.name === pppUsername) || null
    }
  } catch {
    // Silently fail - session data is optional
  } finally {
    isLoadingSession.value = false
  }
}

onMounted(async () => {
  isLoading.value = true
  try {
    await Promise.all([
      store.fetchSubscriber(id.value),
      plansStore.fetchPlans(true),
    ])
    // Fetch active session after subscriber data is loaded
    await fetchActiveSession()
  } finally {
    isLoading.value = false
  }
})

async function handleResetSystemPassword() {
  if (!systemPassword.value || systemPassword.value.length < 8) {
    toast.error('كلمة المرور يجب أن تكون 8 أحرف على الأقل')
    return
  }
  isResettingSystemPw.value = true
  try {
    await store.resetSystemPassword(id.value, systemPassword.value)
    toast.success('تم تغيير كلمة مرور النظام بنجاح')
    systemPassword.value = ''
    showSystemPwForm.value = false
  } catch {
    toast.error('فشل تغيير كلمة مرور النظام — قد لا يكون لدى المشترك حساب نظام')
  } finally {
    isResettingSystemPw.value = false
  }
}

async function handleResetMikroTikPassword() {
  if (!mikroTikPassword.value) {
    toast.error('الرجاء إدخال كلمة المرور الجديدة')
    return
  }
  isResettingMikroTikPw.value = true
  try {
    await store.resetMikroTikPassword(id.value, mikroTikPassword.value)
    toast.success('تم تغيير كلمة مرور MikroTik بنجاح')
    mikroTikPassword.value = ''
    showMikroTikPwForm.value = false
    await store.fetchSubscriber(id.value)
  } catch {
    toast.error('فشل تغيير كلمة مرور MikroTik — تحقق من اتصال MikroTik')
  } finally {
    isResettingMikroTikPw.value = false
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
            
            <!-- Data Usage Section -->
            <div class="mt-3 pt-3 border-t border-jazan-green/20">
              <p class="text-xs font-semibold text-coastal-blue mb-2">استهلاك البيانات (من الاشتراك)</p>
              <div class="grid grid-cols-3 gap-2 text-xs mb-2">
                <div>
                  <span class="text-light-gray">المستهلك:</span>
                  <span class="font-semibold text-charcoal mr-1">{{ formatBytes(s.dataUsedBytes || 0) }}</span>
                </div>
                <div>
                  <span class="text-light-gray">الحد:</span>
                  <span class="font-semibold text-charcoal mr-1">{{ s.dataLimitGB > 0 ? s.dataLimitGB + ' GB' : 'غير محدود' }}</span>
                </div>
                <div>
                  <span class="text-light-gray">المتبقي:</span>
                  <span class="font-semibold text-jazan-green mr-1">{{ getRemainingGB(s.dataUsedBytes || 0, s.dataLimitGB) }}</span>
                </div>
              </div>
              <div v-if="s.dataLimitGB > 0" class="w-full bg-gray-200 rounded-full h-2">
                <div 
                  class="h-2 rounded-full transition-all duration-300"
                  :class="getDataUsagePercent(s.dataUsedBytes || 0, s.dataLimitGB) >= 90 ? 'bg-red-coral' : getDataUsagePercent(s.dataUsedBytes || 0, s.dataLimitGB) >= 70 ? 'bg-warning-yellow' : 'bg-jazan-green'"
                  :style="{ width: getDataUsagePercent(s.dataUsedBytes || 0, s.dataLimitGB) + '%' }"
                ></div>
              </div>
              <p v-if="s.dataLimitExceeded" class="text-red-coral text-xs mt-2 font-semibold">
                ⚠️ تم تجاوز حد البيانات
              </p>
            </div>
            
            <!-- Real-time Session Data from MikroTik -->
            <div v-if="activeSession" class="mt-3 pt-3 border-t border-coastal-blue/20">
              <div class="flex items-center justify-between mb-2">
                <p class="text-xs font-semibold text-coastal-blue">الجلسة الحالية (MikroTik)</p>
                <span class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-jazan-green">
                  <span class="w-1.5 h-1.5 rounded-full bg-jazan-green mr-1 animate-pulse"></span>
                  متصل
                </span>
              </div>
              <div class="grid grid-cols-2 gap-3 text-xs">
                <div class="bg-coastal-blue/5 rounded-lg p-2">
                  <p class="text-light-gray mb-1">التحميل (Download)</p>
                  <p class="font-bold text-coastal-blue text-lg">{{ formatBytes(activeSession.bytesIn) }}</p>
                </div>
                <div class="bg-jazan-green/5 rounded-lg p-2">
                  <p class="text-light-gray mb-1">الرفع (Upload)</p>
                  <p class="font-bold text-jazan-green text-lg">{{ formatBytes(activeSession.bytesOut) }}</p>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3 text-xs mt-2">
                <div>
                  <span class="text-light-gray">الإجمالي:</span>
                  <span class="font-semibold text-charcoal mr-1">{{ formatBytes(activeSession.bytesIn + activeSession.bytesOut) }}</span>
                </div>
                <div>
                  <span class="text-light-gray">مدة الاتصال:</span>
                  <span class="font-semibold text-charcoal mr-1">{{ activeSession.uptime || '—' }}</span>
                </div>
              </div>
              <div v-if="activeSession.address" class="mt-2 text-xs">
                <span class="text-light-gray">عنوان IP:</span>
                <span class="font-mono font-semibold text-charcoal mr-1">{{ activeSession.address }}</span>
              </div>
              <div v-if="activeSession.limitBytesIn && activeSession.limitBytesIn > 0" class="mt-2">
                <p class="text-xs text-light-gray mb-1">حد البيانات (MikroTik):</p>
                <div class="grid grid-cols-2 gap-2 text-xs">
                  <div>
                    <span class="text-light-gray">التحميل:</span>
                    <span class="font-semibold text-charcoal mr-1">{{ formatBytes(activeSession.limitBytesIn) }}</span>
                  </div>
                  <div>
                    <span class="text-light-gray">الرفع:</span>
                    <span class="font-semibold text-charcoal mr-1">{{ formatBytes(activeSession.limitBytesOut || 0) }}</span>
                  </div>
                </div>
              </div>
            </div>
            <div v-else-if="isLoadingSession" class="mt-3 pt-3 border-t border-coastal-blue/20">
              <p class="text-xs text-light-gray">جاري تحميل بيانات الجلسة...</p>
            </div>
            <div v-else class="mt-3 pt-3 border-t border-gray-200">
              <p class="text-xs text-light-gray flex items-center gap-1">
                <span class="w-2 h-2 rounded-full bg-gray-400"></span>
                غير متصل حالياً
              </p>
            </div>
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
            <input v-model="systemPassword" type="password" placeholder="كلمة المرور الجديدة (8 أحرف على الأقل)" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-coastal-blue focus:border-transparent" :disabled="isResettingSystemPw" />
            <button class="w-full px-4 py-2 bg-coastal-blue text-white rounded-lg text-sm hover:bg-coastal-blue-dark transition-colors disabled:opacity-50" :disabled="isResettingSystemPw" @click="handleResetSystemPassword">
              <span v-if="isResettingSystemPw">جاري الحفظ...</span>
              <span v-else>حفظ كلمة مرور النظام</span>
            </button>
          </div>
        </div>

        <!-- MikroTik Password -->
        <div class="border border-gray-200 rounded-xl p-4">
          <div class="flex items-center justify-between mb-3">
            <div>
              <h4 class="font-semibold text-charcoal text-sm">كلمة مرور MikroTik</h4>
              <p class="text-xs text-light-gray">لاتصال PPPoE بالراوتر</p>
            </div>
            <button class="text-xs px-3 py-1 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors" :disabled="isResettingMikroTikPw" @click="showMikroTikPwForm = !showMikroTikPwForm">
              {{ showMikroTikPwForm ? 'إلغاء' : 'تغيير' }}
            </button>
          </div>
          <div v-if="showMikroTikPwForm" class="space-y-2">
            <input v-model="mikroTikPassword" type="text" placeholder="كلمة مرور MikroTik الجديدة" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-jazan-green focus:border-transparent" :disabled="isResettingMikroTikPw" />
            <p class="text-xs text-light-gray">ملاحظة: كلمة المرور ستُحفظ كنص عادي في MikroTik</p>
            <button class="w-full px-4 py-2 bg-jazan-green text-white rounded-lg text-sm hover:bg-jazan-green-dark transition-colors disabled:opacity-50" :disabled="isResettingMikroTikPw" @click="handleResetMikroTikPassword">
              <span v-if="isResettingMikroTikPw">جاري الحفظ والمزامنة...</span>
              <span v-else>حفظ كلمة مرور MikroTik</span>
            </button>
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
