<script setup lang="ts">
import { ref } from 'vue'
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
const name = ref('')
const email = ref('')
const message = ref('')

function handleSubmit() {
  if (!name.value.trim() || !email.value.trim() || !message.value.trim()) {
    toast.error('الرجاء ملء جميع الحقول')
    return
  }
  toast.success('تم إرسال رسالتك بنجاح. سيتم التواصل معك قريباً.')
  name.value = ''
  email.value = ''
  message.value = ''
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-soft-beige-light px-4" dir="rtl">
    <div class="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 border border-soft-beige">
      <h2 class="text-2xl font-bold text-coastal-blue mb-6 text-center">تواصل مع المسؤول</h2>

      <form class="space-y-6" @submit.prevent="handleSubmit">
        <div>
          <label class="block text-sm font-medium text-charcoal mb-2">الاسم</label>
          <input v-model="name" type="text" required class="w-full px-4 py-3 bg-soft-beige-light border border-sandy-brown/30 rounded-xl text-charcoal focus:ring-2 focus:ring-jazan-green focus:border-jazan-green transition-colors" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-2">البريد الإلكتروني</label>
          <input v-model="email" type="email" required class="w-full px-4 py-3 bg-soft-beige-light border border-sandy-brown/30 rounded-xl text-charcoal focus:ring-2 focus:ring-jazan-green focus:border-jazan-green transition-colors" />
        </div>
        <div>
          <label class="block text-sm font-medium text-charcoal mb-2">الرسالة</label>
          <textarea v-model="message" rows="4" required class="w-full px-4 py-3 bg-soft-beige-light border border-sandy-brown/30 rounded-xl text-charcoal focus:ring-2 focus:ring-jazan-green focus:border-jazan-green transition-colors"></textarea>
        </div>
        <button type="submit" class="w-full py-3 px-4 bg-jazan-green text-white rounded-xl hover:bg-jazan-green-dark transition-colors font-semibold shadow-lg shadow-jazan-green/25">إرسال</button>
        <div class="text-center">
          <RouterLink to="/" class="text-sm text-sandy-brown hover:text-sandy-brown-light font-medium">العودة لتسجيل الدخول</RouterLink>
        </div>
      </form>
    </div>
  </div>
</template>
