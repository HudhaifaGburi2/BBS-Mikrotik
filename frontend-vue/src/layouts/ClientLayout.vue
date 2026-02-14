<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

const navItems = [
  { label: 'لوحة التحكم', to: '/client/dashboard' },
  { label: 'اشتراكي', to: '/client/subscription' },
  { label: 'الباقات', to: '/client/plans' },
  { label: 'الفواتير', to: '/client/invoices' },
  { label: 'الدفع', to: '/client/payment' },
  { label: 'الاستهلاك', to: '/client/usage' },
]

async function handleLogout() {
  await auth.logout()
  router.replace('/')
}
</script>

<template>
  <div class="min-h-screen bg-soft-beige-light" dir="rtl">
    <header class="bg-coastal-blue shadow-md">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <div class="flex items-center gap-2">
            <svg class="h-8 w-8 shrink-0" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
              <rect width="64" height="64" rx="14" fill="#E8A945"/>
              <text x="32" y="46" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="40" fill="#2A4B7C">D</text>
            </svg>
            <h1 class="text-lg font-bold text-golden-sand">DOSHI</h1>
          </div>
          <nav class="hidden md:flex items-center gap-6">
            <RouterLink
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              class="text-sm text-pale-olive hover:text-white transition-colors"
              active-class="!text-golden-sand font-semibold"
            >
              {{ item.label }}
            </RouterLink>
          </nav>
          <div class="flex items-center gap-4">
            <span class="text-sm text-pale-olive">{{ auth.fullName }}</span>
            <button
              class="text-sm text-golden-sand-light hover:text-white transition-colors"
              @click="handleLogout"
            >
              خروج
            </button>
          </div>
        </div>
      </div>
    </header>
    <main class="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
      <RouterView />
    </main>
  </div>
</template>
