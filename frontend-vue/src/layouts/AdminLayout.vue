<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const sidebarOpen = ref(false)

const navItems = [
  { label: 'لوحة التحكم', icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6', to: '/admin/dashboard' },
  { label: 'المشتركون', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z', to: '/admin/subscribers' },
  { label: 'الباقات', icon: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10', to: '/admin/plans' },
  { label: 'الفواتير', icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', to: '/admin/invoices' },
  { label: 'المدفوعات', icon: 'M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z', to: '/admin/payments' },
  { label: 'المتصلون', icon: 'M5.636 18.364a9 9 0 010-12.728m12.728 0a9 9 0 010 12.728m-9.9-2.829a5 5 0 010-7.07m7.072 0a5 5 0 010 7.07M13 12a1 1 0 11-2 0 1 1 0 012 0z', to: '/admin/online-users' },
  { label: 'التقارير', icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z', to: '/admin/reports' },
  { label: 'المستخدمون', icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z', to: '/admin/users' },
  { label: 'مستخدمو PPP', icon: 'M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z', to: '/admin/mikrotik/users' },
  { label: 'بروفايلات PPP', icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z', to: '/admin/mikrotik/profiles' },
]

async function handleLogout() {
  await auth.logout()
  router.replace('/')
}
</script>

<template>
  <div class="min-h-screen bg-soft-beige-light" dir="rtl">
    <!-- Sidebar -->
    <aside
      :class="[
        'fixed inset-y-0 right-0 z-20 w-64 bg-coastal-blue text-white transform transition-transform duration-200 ease-in-out lg:translate-x-0',
        sidebarOpen ? 'translate-x-0' : 'translate-x-full lg:translate-x-0',
      ]"
    >
      <div class="flex items-center justify-between h-16 px-6 border-b border-coastal-blue-light/30">
        <div class="flex items-center gap-2">
          <svg class="h-8 w-8 shrink-0" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
            <rect width="64" height="64" rx="14" fill="#E8A945"/>
            <text x="32" y="46" text-anchor="middle" font-family="Arial, sans-serif" font-weight="bold" font-size="40" fill="#2A4B7C">D</text>
          </svg>
          <h1 class="text-lg font-bold text-golden-sand">Dushi</h1>
        </div>
        <button class="lg:hidden text-pale-olive hover:text-white" @click="sidebarOpen = false">
          <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <nav class="mt-6 px-3 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-pale-olive hover:bg-coastal-blue-dark hover:text-white transition-colors"
          active-class="!bg-jazan-green !text-white"
          @click="sidebarOpen = false"
        >
          <svg class="h-5 w-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="item.icon" />
          </svg>
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="absolute bottom-0 w-full p-4 border-t border-coastal-blue-light/30">
        <div class="flex items-center gap-3 mb-3">
          <div class="h-8 w-8 rounded-full bg-golden-sand flex items-center justify-center text-sm font-bold text-coastal-blue-dark">
            {{ auth.fullName.charAt(0) }}
          </div>
          <span class="text-sm text-pale-olive truncate">{{ auth.fullName }}</span>
        </div>
        <button
          class="w-full flex items-center gap-2 px-3 py-2 text-sm text-pale-olive hover:text-white hover:bg-coastal-blue-dark rounded-lg transition-colors"
          @click="handleLogout"
        >
          <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
          </svg>
          تسجيل الخروج
        </button>
      </div>
    </aside>

    <!-- Main content -->
    <main class="lg:mr-64 min-h-screen">
      <!-- Top Nav Bar -->
      <header class="bg-white shadow-sm border-b border-soft-beige sticky top-0 z-10">
        <div class="flex items-center justify-between px-6 h-14">
          <div class="flex items-center gap-3">
            <button class="lg:hidden p-1.5 rounded-lg hover:bg-gray-100" @click="sidebarOpen = !sidebarOpen">
              <svg class="h-5 w-5 text-charcoal" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
          </div>
          <div class="flex items-center gap-4">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-full bg-coastal-blue flex items-center justify-center text-sm font-bold text-white">
                {{ auth.fullName.charAt(0) }}
              </div>
              <span class="text-sm font-medium text-charcoal hidden sm:inline">{{ auth.fullName }}</span>
            </div>
            <button
              class="flex items-center gap-1.5 px-3 py-1.5 text-sm text-red-coral hover:bg-red-coral/10 rounded-lg transition-colors"
              @click="handleLogout"
            >
              <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              <span class="hidden sm:inline">خروج</span>
            </button>
          </div>
        </div>
      </header>
      <div class="p-6">
        <RouterView />
      </div>
    </main>
  </div>
</template>
