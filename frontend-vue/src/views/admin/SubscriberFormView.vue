<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSubscribersStore } from '@/stores/subscribers'
import { usePlansStore } from '@/stores/plans'
import { useToastStore } from '@/stores/toast'
import { useValidation } from '@/composables/useValidation'
import FormField from '@/components/FormField.vue'
import AppLoader from '@/components/AppLoader.vue'
import type { CreateSubscriberCommand } from '@/types'

const route = useRoute()
const router = useRouter()
const subscribersStore = useSubscribersStore()
const plansStore = usePlansStore()
const toast = useToastStore()
const { errors, validateEmail, validatePhone, sanitize, clearErrors } = useValidation()

const isEdit = computed(() => !!route.params.id)
const isLoading = ref(false)

const form = ref({
  fullName: '',
  email: '',
  phoneNumber: '',
  nationalId: '',
  address: '',
  macAddress: '',
  ipAddress: '',
  planId: '',
  startDate: new Date().toISOString().split('T')[0],
  autoRenew: false,
  mikroTikUsername: '',
  mikroTikPassword: '',
  createSystemAccount: false,
  systemUsername: '',
  systemPassword: '',
})

const macRegex = /^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$/
const ipRegex = /^(\d{1,3}\.){3}\d{1,3}$/

const selectedPlan = computed(() => {
  if (!form.value.planId) return null
  return plansStore.plans.find(p => p.id === form.value.planId) || null
})

const showMikroTikSection = computed(() => !isEdit.value && form.value.planId)

function generateUsername(fullName: string): string {
  const sanitized = fullName
    .replace(/[^\p{L}\p{N}\s]/gu, '')
    .replace(/\s+/g, '_')
    .toLowerCase()
    .slice(0, 12)
  const suffix = Math.floor(1000 + Math.random() * 9000)
  return `${sanitized}_${suffix}`
}

function generatePassword(): string {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*'
  let password = ''
  for (let i = 0; i < 12; i++) {
    password += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return password
}

watch(() => form.value.planId, (planId) => {
  // Auto-generate credentials when plan is selected and no credentials exist
  if (planId && !form.value.mikroTikUsername) {
    if (form.value.fullName) {
      form.value.mikroTikUsername = generateUsername(form.value.fullName)
      form.value.mikroTikPassword = generatePassword()
    }
  }
})

function regenerateCredentials() {
  if (form.value.fullName) {
    form.value.mikroTikUsername = generateUsername(form.value.fullName)
    form.value.mikroTikPassword = generatePassword()
    toast.success('تم إنشاء بيانات MikroTik جديدة')
  } else {
    toast.error('الرجاء إدخال اسم المشترك أولاً')
  }
}

function validate(): boolean {
  clearErrors()
  if (!form.value.fullName.trim()) errors.value.fullName = 'الاسم الكامل مطلوب'
  if (!form.value.email.trim()) errors.value.email = 'البريد الإلكتروني مطلوب'
  else if (!validateEmail(form.value.email)) errors.value.email = 'الرجاء إدخال عنوان بريد إلكتروني صحيح'
  if (!form.value.phoneNumber.trim()) errors.value.phoneNumber = 'رقم الهاتف مطلوب'
  else if (!validatePhone(form.value.phoneNumber)) errors.value.phoneNumber = 'الرجاء إدخال رقم هاتف صحيح'
  if (!form.value.address.trim()) errors.value.address = 'العنوان مطلوب'
  if (form.value.macAddress && !macRegex.test(form.value.macAddress)) errors.value.macAddress = 'صيغة MAC غير صحيحة (مثال: AA:BB:CC:DD:EE:FF)'
  if (form.value.ipAddress && !ipRegex.test(form.value.ipAddress)) errors.value.ipAddress = 'صيغة IP غير صحيحة (مثال: 192.168.1.100)'

  if (!isEdit.value) {
    // Validate MikroTik credentials if plan is selected
    if (form.value.planId) {
      if (!form.value.mikroTikUsername.trim()) errors.value.mikroTikUsername = 'اسم مستخدم MikroTik مطلوب'
      if (!form.value.mikroTikPassword) errors.value.mikroTikPassword = 'كلمة مرور MikroTik مطلوبة'
    }

    if (form.value.createSystemAccount) {
      if (!form.value.systemUsername.trim()) errors.value.systemUsername = 'اسم مستخدم النظام مطلوب'
      if (!form.value.systemPassword) errors.value.systemPassword = 'كلمة مرور النظام مطلوبة'
      else if (form.value.systemPassword.length < 8) errors.value.systemPassword = 'كلمة المرور يجب أن تكون 8 أحرف على الأقل'
      if (form.value.mikroTikPassword && form.value.systemPassword === form.value.mikroTikPassword) errors.value.systemPassword = 'كلمة مرور النظام يجب أن تختلف عن كلمة مرور MikroTik'
    }
  }
  return Object.keys(errors.value).length === 0
}

async function handleSubmit() {
  if (!validate()) {
    toast.error('الرجاء تصحيح الأخطاء في النموذج')
    return
  }

  isLoading.value = true
  try {
    if (isEdit.value) {
      await subscribersStore.updateSubscriber(String(route.params.id), {
        fullName: sanitize(form.value.fullName),
        email: sanitize(form.value.email),
        phoneNumber: sanitize(form.value.phoneNumber),
        nationalId: form.value.nationalId ? sanitize(form.value.nationalId) : null,
        address: sanitize(form.value.address),
        macAddress: form.value.macAddress ? sanitize(form.value.macAddress) : null,
        ipAddress: form.value.ipAddress ? sanitize(form.value.ipAddress) : null,
      })
      toast.success('تم تحديث المشترك بنجاح')
    } else {
      const command: CreateSubscriberCommand = {
        fullName: sanitize(form.value.fullName),
        email: sanitize(form.value.email),
        phoneNumber: sanitize(form.value.phoneNumber),
        nationalId: form.value.nationalId ? sanitize(form.value.nationalId) : null,
        address: sanitize(form.value.address),
        macAddress: form.value.macAddress ? sanitize(form.value.macAddress) : null,
        ipAddress: form.value.ipAddress ? sanitize(form.value.ipAddress) : null,
        planId: form.value.planId || null,
        startDate: form.value.planId ? form.value.startDate : null,
        autoRenew: form.value.autoRenew,
        autoCreateMikroTik: false,
        pppUsername: form.value.mikroTikUsername ? sanitize(form.value.mikroTikUsername) : null,
        pppPassword: form.value.mikroTikPassword ? form.value.mikroTikPassword : null,
        createSystemAccount: form.value.createSystemAccount,
        systemUsername: form.value.createSystemAccount && form.value.systemUsername ? sanitize(form.value.systemUsername) : null,
        systemPassword: form.value.createSystemAccount && form.value.systemPassword ? form.value.systemPassword : null,
      }
      await subscribersStore.createSubscriber(command)
      toast.success('تم إنشاء المشترك بنجاح')
    }
    router.push('/admin/subscribers')
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'فشل حفظ المشترك'
    toast.error(message)
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await plansStore.fetchPlans()
  if (isEdit.value) {
    isLoading.value = true
    try {
      await subscribersStore.fetchSubscriber(route.params.id as string)
      const sub = subscribersStore.currentSubscriber
      if (sub) {
        form.value.fullName = sub.fullName
        form.value.email = sub.email
        form.value.phoneNumber = sub.phoneNumber
        form.value.nationalId = sub.nationalId || ''
        form.value.address = sub.address
        form.value.macAddress = sub.macAddress || ''
        form.value.ipAddress = sub.ipAddress || ''
      }
    } finally {
      isLoading.value = false
    }
  }
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="max-w-3xl mx-auto space-y-6">
    <div class="bg-white rounded-xl shadow-md p-8 border border-soft-beige">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-2xl font-bold text-coastal-blue">{{ isEdit ? 'تعديل مشترك' : 'إضافة مشترك' }}</h2>
        <RouterLink to="/admin/subscribers" class="text-light-gray hover:text-charcoal">
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </RouterLink>
      </div>

      <form class="space-y-6" @submit.prevent="handleSubmit">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField label="الاسم الكامل" :error="errors.fullName" required>
            <input v-model="form.fullName" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="البريد الإلكتروني" :error="errors.email" required>
            <input v-model="form.email" type="email" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="رقم الهاتف" :error="errors.phoneNumber" required>
            <input v-model="form.phoneNumber" type="tel" placeholder="+966 5XXXXXXXX" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="رقم الهوية">
            <input v-model="form.nationalId" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
        </div>

        <FormField label="العنوان" :error="errors.address" required>
          <textarea v-model="form.address" rows="3" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green"></textarea>
        </FormField>

        <!-- Network Info -->
        <div class="pt-6 border-t border-soft-beige">
          <h3 class="text-lg font-semibold text-coastal-blue mb-4">معلومات الشبكة</h3>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField label="عنوان MAC" :error="errors.macAddress">
              <input v-model="form.macAddress" type="text" placeholder="AA:BB:CC:DD:EE:FF" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green font-mono" />
            </FormField>
            <FormField label="عنوان IP" :error="errors.ipAddress">
              <input v-model="form.ipAddress" type="text" placeholder="192.168.1.100" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green font-mono" />
            </FormField>
          </div>
        </div>

        <!-- Subscription Details (new subscriber only) -->
        <div v-if="!isEdit" class="pt-6 border-t border-soft-beige">
          <h3 class="text-lg font-semibold text-jazan-green mb-4">تفاصيل الاشتراك</h3>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField label="الباقة" :error="errors.planId">
              <select v-model="form.planId" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green">
                <option value="">اختر باقة...</option>
                <option v-for="plan in plansStore.plans" :key="plan.id" :value="plan.id">
                  {{ plan.name }} - {{ plan.price }} {{ plan.currency }} ({{ plan.speedMbps }} Mbps)
                </option>
              </select>
            </FormField>
            <FormField label="تاريخ البداية">
              <input v-model="form.startDate" type="date" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
            </FormField>
          </div>
          <label class="flex items-center mt-4">
            <input v-model="form.autoRenew" type="checkbox" class="h-4 w-4 text-jazan-green focus:ring-jazan-green border-sandy-brown rounded" />
            <span class="mr-2 text-sm text-charcoal">تفعيل التجديد التلقائي</span>
          </label>
        </div>

        <!-- MikroTik PPPoE Credentials (new subscriber only, when plan selected) -->
        <div v-if="showMikroTikSection" class="pt-6 border-t border-soft-beige">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h3 class="text-lg font-semibold text-jazan-green">بيانات MikroTik (PPPoE)</h3>
              <p class="text-sm text-light-gray">
                سيتم إنشاء حساب PPPoE وربطه بباقة 
                <strong class="text-jazan-green">{{ selectedPlan?.name }}</strong>
                ({{ selectedPlan?.mikroTikProfileName }})
              </p>
            </div>
            <button type="button" @click="regenerateCredentials" class="px-3 py-2 bg-coastal-blue/10 text-coastal-blue rounded-lg hover:bg-coastal-blue/20 transition-colors text-sm flex items-center gap-2" title="إنشاء تلقائي">
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              <span>إنشاء تلقائي</span>
            </button>
          </div>

          <!-- Credentials entry - always visible -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField label="اسم مستخدم MikroTik" :error="errors.mikroTikUsername" required>
              <input v-model="form.mikroTikUsername" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green font-mono" placeholder="pppoe_user" />
            </FormField>
            <FormField label="كلمة مرور MikroTik" :error="errors.mikroTikPassword" required>
              <input v-model="form.mikroTikPassword" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green font-mono" placeholder="كلمة المرور" />
            </FormField>
          </div>
        </div>

        <!-- System Account Creation (new subscriber only) -->
        <div v-if="!isEdit" class="pt-6 border-t border-soft-beige">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-coastal-blue">حساب تسجيل الدخول للنظام</h3>
            <label class="flex items-center gap-2">
              <input v-model="form.createSystemAccount" type="checkbox" class="h-4 w-4 text-jazan-green focus:ring-jazan-green border-sandy-brown rounded" />
              <span class="text-sm text-charcoal">إنشاء حساب نظام</span>
            </label>
          </div>
          <div v-if="form.createSystemAccount">
            <div class="bg-warning-yellow/10 border border-warning-yellow/30 rounded-lg p-3 mb-4">
              <p class="text-sm text-golden-sand-dark">
                <strong>تنبيه:</strong> كلمة مرور النظام يجب أن تكون مختلفة عن كلمة مرور MikroTik لأسباب أمنية.
              </p>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <FormField label="اسم مستخدم النظام" :error="errors.systemUsername" required>
                <input v-model="form.systemUsername" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" placeholder="اسم الدخول للنظام" />
              </FormField>
              <FormField label="كلمة مرور النظام" :error="errors.systemPassword" required>
                <input v-model="form.systemPassword" type="password" autocomplete="new-password" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" placeholder="8 أحرف على الأقل — مختلفة عن MikroTik" />
              </FormField>
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end gap-4 pt-6 border-t border-soft-beige">
          <RouterLink to="/admin/subscribers" class="px-6 py-2 bg-pale-olive/30 text-charcoal rounded-lg hover:bg-pale-olive/50 transition-colors">إلغاء</RouterLink>
          <button type="submit" :disabled="isLoading" class="px-6 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors disabled:opacity-50 shadow-md">حفظ المشترك</button>
        </div>
      </form>
    </div>
  </div>
</template>
