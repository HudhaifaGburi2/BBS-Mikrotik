<script setup lang="ts">
import { ref } from 'vue'
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
const email = ref('')
const submitted = ref(false)

function handleSubmit() {
  if (!email.value.trim()) {
    toast.error('الرجاء إدخال البريد الإلكتروني')
    return
  }
  submitted.value = true
  toast.success('تم إرسال رابط إعادة تعيين كلمة المرور إلى بريدك الإلكتروني')
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-soft-beige-light px-4" dir="rtl">
    <div class="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 border border-soft-beige">
      <h2 class="text-2xl font-bold text-coastal-blue mb-6 text-center">استعادة كلمة المرور</h2>

      <div v-if="submitted" class="text-center text-teal">
        <p>تم إرسال رابط إعادة التعيين إلى بريدك الإلكتروني.</p>
        <RouterLink to="/" class="mt-4 inline-block text-coastal-blue hover:text-coastal-blue-dark font-medium">العودة لتسجيل الدخول</RouterLink>
      </div>

      <form v-else class="space-y-6" @submit.prevent="handleSubmit">
        <div>
          <label for="email" class="block text-sm font-medium text-charcoal mb-2">البريد الإلكتروني</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            class="w-full px-4 py-3 bg-soft-beige-light border border-sandy-brown/30 rounded-xl text-charcoal placeholder-light-gray focus:ring-2 focus:ring-jazan-green focus:border-jazan-green transition-colors"
            placeholder="أدخل بريدك الإلكتروني"
          />
        </div>
        <button type="submit" class="w-full py-3 px-4 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors font-semibold shadow-lg shadow-jazan-green/25">
          إرسال رابط الاستعادة
        </button>
        <div class="text-center">
          <RouterLink to="/" class="text-sm text-sandy-brown hover:text-sandy-brown-light font-medium">العودة لتسجيل الدخول</RouterLink>
        </div>
      </form>
    </div>
  </div>
</template>
