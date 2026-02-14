<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUsersStore } from '@/stores/users'
import { useSubscribersStore } from '@/stores/subscribers'
import { useToastStore } from '@/stores/toast'
import { useValidation } from '@/composables/useValidation'
import AppLoader from '@/components/AppLoader.vue'
import FormField from '@/components/FormField.vue'
import type { SystemUserRole } from '@/types'

const route = useRoute()
const router = useRouter()
const usersStore = useUsersStore()
const subscribersStore = useSubscribersStore()
const toast = useToastStore()
const { sanitize, errors, clearErrors } = useValidation()

const isEdit = computed(() => route.params.id && route.params.id !== 'new')
const isLoading = ref(false)
const showResetPassword = ref(false)

const form = ref({
  username: '',
  email: '',
  fullName: '',
  password: '',
  confirmPassword: '',
  role: 'Client' as SystemUserRole,
  subscriberId: null as string | null,
  isActive: true,
})

const resetForm = ref({
  newPassword: '',
  confirmPassword: '',
})

function validate(): boolean {
  clearErrors()
  if (!isEdit.value && !form.value.username.trim()) errors.value.username = 'اسم المستخدم مطلوب'
  if (!form.value.fullName.trim()) errors.value.fullName = 'الاسم الكامل مطلوب'
  if (!form.value.email.trim()) errors.value.email = 'البريد الإلكتروني مطلوب'
  if (!isEdit.value) {
    if (!form.value.password) errors.value.password = 'كلمة المرور مطلوبة'
    else if (form.value.password.length < 8) errors.value.password = 'كلمة المرور يجب أن تكون 8 أحرف على الأقل'
    if (form.value.password !== form.value.confirmPassword) errors.value.confirmPassword = 'كلمات المرور غير متطابقة'
  }
  return Object.keys(errors.value).length === 0
}

function validateReset(): boolean {
  clearErrors()
  if (!resetForm.value.newPassword) errors.value.newPassword = 'كلمة المرور الجديدة مطلوبة'
  else if (resetForm.value.newPassword.length < 8) errors.value.newPassword = 'كلمة المرور يجب أن تكون 8 أحرف على الأقل'
  if (resetForm.value.newPassword !== resetForm.value.confirmPassword) errors.value.confirmResetPassword = 'كلمات المرور غير متطابقة'
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
      await usersStore.updateUser(String(route.params.id), {
        email: sanitize(form.value.email),
        fullName: sanitize(form.value.fullName),
        role: form.value.role,
        isActive: form.value.isActive,
      })
      toast.success('تم تحديث المستخدم بنجاح')
    } else {
      await usersStore.createUser({
        username: sanitize(form.value.username),
        email: sanitize(form.value.email),
        fullName: sanitize(form.value.fullName),
        password: form.value.password,
        confirmPassword: form.value.confirmPassword,
        role: form.value.role,
        subscriberId: form.value.role === 'Client' ? form.value.subscriberId : null,
      })
      toast.success('تم إنشاء المستخدم بنجاح')
    }
    router.push('/admin/users')
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'فشل حفظ المستخدم'
    toast.error(message)
  } finally {
    isLoading.value = false
  }
}

async function handleResetPassword() {
  if (!validateReset()) {
    toast.error('الرجاء تصحيح الأخطاء')
    return
  }
  isLoading.value = true
  try {
    await usersStore.resetPassword(String(route.params.id), {
      newPassword: resetForm.value.newPassword,
      confirmPassword: resetForm.value.confirmPassword,
    })
    toast.success('تم إعادة تعيين كلمة المرور بنجاح')
    showResetPassword.value = false
    resetForm.value = { newPassword: '', confirmPassword: '' }
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'فشل إعادة تعيين كلمة المرور'
    toast.error(message)
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  if (isEdit.value) {
    isLoading.value = true
    try {
      await usersStore.fetchById(String(route.params.id))
      if (usersStore.currentUser) {
        form.value.username = usersStore.currentUser.username
        form.value.email = usersStore.currentUser.email
        form.value.fullName = usersStore.currentUser.fullName
        form.value.role = usersStore.currentUser.role
        form.value.subscriberId = usersStore.currentUser.subscriberId
        form.value.isActive = usersStore.currentUser.isActive
      }
    } finally {
      isLoading.value = false
    }
  }
  await subscribersStore.fetchSubscribers(1, 100)
})
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="max-w-3xl mx-auto">
    <div class="bg-white rounded-xl shadow-md p-8 border border-soft-beige">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-2xl font-bold text-coastal-blue">{{ isEdit ? 'تعديل مستخدم' : 'إضافة مستخدم جديد' }}</h2>
        <RouterLink to="/admin/users" class="text-light-gray hover:text-charcoal">
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </RouterLink>
      </div>

      <form class="space-y-6" @submit.prevent="handleSubmit">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField label="اسم المستخدم" :error="errors.username" :required="!isEdit">
            <input
              v-model="form.username"
              type="text"
              :disabled="!!isEdit"
              :class="[
                'w-full px-4 py-2 rounded-lg transition-colors',
                isEdit
                  ? 'bg-pale-olive/20 border border-pale-olive text-light-gray cursor-not-allowed'
                  : 'bg-soft-beige-light border border-sandy-brown/30 focus:ring-2 focus:ring-jazan-green focus:border-jazan-green'
              ]"
              placeholder="اسم الدخول للنظام"
            />
          </FormField>
          <FormField label="الاسم الكامل" :error="errors.fullName" required>
            <input v-model="form.fullName" type="text" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="البريد الإلكتروني" :error="errors.email" required>
            <input v-model="form.email" type="email" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
          <FormField label="الدور" required>
            <select v-model="form.role" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green">
              <option value="Admin">مسؤول</option>
              <option value="Client">عميل</option>
            </select>
          </FormField>
        </div>

        <div v-if="form.role === 'Client'" class="pt-4 border-t border-soft-beige">
          <FormField label="ربط بمشترك (اختياري)">
            <select v-model="form.subscriberId" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green">
              <option :value="null">— بدون ربط —</option>
              <option v-for="sub in subscribersStore.subscribers" :key="sub.id" :value="sub.id">
                {{ sub.fullName }} ({{ sub.email }})
              </option>
            </select>
          </FormField>
        </div>

        <div v-if="!isEdit" class="pt-4 border-t border-soft-beige">
          <h3 class="text-lg font-semibold text-jazan-green mb-4">كلمة مرور النظام</h3>
          <p class="text-sm text-light-gray mb-4">هذه كلمة مرور تسجيل الدخول للنظام — مختلفة عن كلمة مرور MikroTik</p>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField label="كلمة المرور" :error="errors.password" required>
              <input v-model="form.password" type="password" autocomplete="new-password" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" placeholder="8 أحرف على الأقل" />
            </FormField>
            <FormField label="تأكيد كلمة المرور" :error="errors.confirmPassword" required>
              <input v-model="form.confirmPassword" type="password" autocomplete="new-password" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
            </FormField>
          </div>
        </div>

        <div v-if="isEdit" class="pt-4 border-t border-soft-beige">
          <label class="flex items-center gap-2">
            <input v-model="form.isActive" type="checkbox" class="h-4 w-4 text-jazan-green focus:ring-jazan-green border-sandy-brown rounded" />
            <span class="text-sm text-charcoal">الحساب مفعّل</span>
          </label>
        </div>

        <div class="flex items-center justify-end gap-4 pt-6 border-t border-soft-beige">
          <RouterLink to="/admin/users" class="px-6 py-2 bg-pale-olive/30 text-charcoal rounded-lg hover:bg-pale-olive/50 transition-colors">إلغاء</RouterLink>
          <button type="submit" :disabled="isLoading" class="px-6 py-2 bg-jazan-green text-white rounded-lg hover:bg-jazan-green-dark transition-colors disabled:opacity-50 shadow-md">
            {{ isEdit ? 'تحديث المستخدم' : 'إنشاء المستخدم' }}
          </button>
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="bg-white rounded-xl shadow-md p-8 border border-soft-beige mt-6">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold text-coastal-blue">إعادة تعيين كلمة المرور</h3>
        <button
          class="px-4 py-2 text-sm bg-golden-sand/15 text-golden-sand-dark rounded-lg hover:bg-golden-sand/25 transition-colors"
          @click="showResetPassword = !showResetPassword"
        >
          {{ showResetPassword ? 'إلغاء' : 'إعادة تعيين' }}
        </button>
      </div>
      <p class="text-sm text-light-gray mb-4">تغيير كلمة مرور تسجيل الدخول للنظام فقط — لا يؤثر على كلمة مرور MikroTik</p>

      <form v-if="showResetPassword" class="space-y-4" @submit.prevent="handleResetPassword">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField label="كلمة المرور الجديدة" :error="errors.newPassword" required>
            <input v-model="resetForm.newPassword" type="password" autocomplete="new-password" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" placeholder="8 أحرف على الأقل" />
          </FormField>
          <FormField label="تأكيد كلمة المرور" :error="errors.confirmResetPassword" required>
            <input v-model="resetForm.confirmPassword" type="password" autocomplete="new-password" class="w-full px-4 py-2 bg-soft-beige-light border border-sandy-brown/30 rounded-lg focus:ring-2 focus:ring-jazan-green focus:border-jazan-green" />
          </FormField>
        </div>
        <div class="flex justify-end">
          <button type="submit" :disabled="isLoading" class="px-6 py-2 bg-golden-sand text-white rounded-lg hover:bg-golden-sand-dark transition-colors disabled:opacity-50 shadow-md">
            حفظ كلمة المرور الجديدة
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
