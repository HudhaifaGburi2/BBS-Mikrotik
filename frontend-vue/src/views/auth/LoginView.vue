<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import AppLoader from '@/components/AppLoader.vue'

const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()

const username = ref('')
const password = ref('')
const rememberMe = ref(false)
const isLoading = ref(false)

async function handleLogin() {
  if (!username.value.trim() || !password.value) {
    toast.error('الرجاء إدخال اسم المستخدم وكلمة المرور')
    return
  }

  isLoading.value = true
  try {
    const userType = await auth.login(username.value.trim(), password.value, rememberMe.value)
    toast.success('تم تسجيل الدخول بنجاح!')
    const target = userType === 'Admin' ? '/admin/dashboard' : '/client/dashboard'
    router.replace(target)
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : 'فشل تسجيل الدخول. الرجاء التحقق من بيانات الاعتماد.'
    toast.error(message)
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <AppLoader :visible="isLoading" />
  <div class="login-page min-h-screen flex items-center justify-center px-4" dir="rtl">
    <div class="login-overlay"></div>
    <div class="relative z-10 max-w-md w-full bg-white/95 backdrop-blur-sm rounded-2xl shadow-2xl p-8 border border-soft-beige">
      <div class="text-center mb-8">
        <svg class="h-16 w-16 mx-auto mb-4" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
          <rect width="64" height="64" rx="14" fill="#E8A945"/>
          <text x="32" y="46" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="40" fill="#2A4B7C">D</text>
        </svg>
        <h1 class="text-3xl font-bold text-coastal-blue">Dushi</h1>
        <p class="text-light-gray mt-2">تسجيل الدخول إلى حسابك</p>
      </div>

      <form class="space-y-6" @submit.prevent="handleLogin">
        <div>
          <label for="username" class="block text-sm font-medium text-charcoal mb-2">اسم المستخدم</label>
          <input
            id="username"
            v-model="username"
            type="text"
            required
            autocomplete="username"
            class="w-full px-4 py-3 bg-soft-beige-light border border-sandy-brown/30 rounded-xl text-charcoal placeholder-light-gray focus:ring-2 focus:ring-jazan-green focus:border-jazan-green transition-colors"
            placeholder="أدخل اسم المستخدم"
          />
        </div>

        <div>
          <label for="password" class="block text-sm font-medium text-charcoal mb-2">كلمة المرور</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            class="w-full px-4 py-3 bg-soft-beige-light border border-sandy-brown/30 rounded-xl text-charcoal placeholder-light-gray focus:ring-2 focus:ring-jazan-green focus:border-jazan-green transition-colors"
            placeholder="أدخل كلمة المرور"
          />
        </div>

        <div class="flex items-center justify-between">
          <label class="flex items-center">
            <input v-model="rememberMe" type="checkbox" class="h-4 w-4 text-jazan-green focus:ring-jazan-green border-sandy-brown rounded" />
            <span class="mr-2 text-sm text-charcoal">تذكرني</span>
          </label>
          <RouterLink to="/forgot-password" class="text-sm text-coastal-blue hover:text-coastal-blue-dark font-medium">نسيت كلمة المرور؟</RouterLink>
        </div>

        <button
          type="submit"
          :disabled="isLoading"
          class="w-full py-3 px-4 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors disabled:opacity-50 font-semibold shadow-lg shadow-jazan-green/25"
        >
          تسجيل الدخول
        </button>
      </form>

      <div class="mt-6 text-center">
        <RouterLink to="/contact-admin" class="text-sm text-sandy-brown hover:text-sandy-brown-light font-medium">تواصل مع المسؤول</RouterLink>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  position: relative;
  background-image: url('/landingpage.jpg');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

.login-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(42, 75, 124, 0.75) 0%, rgba(59, 127, 63, 0.55) 100%);
  z-index: 1;
}
</style>
