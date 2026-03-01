import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import http from '@/services/http'
import type { UserData, UserType } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserData | null>(null)
  const isLoading = ref(false)
  const sessionChecked = ref(false)

  const isAuthenticated = computed(() => user.value !== null)
  const isAdmin = computed(() => user.value?.userType === 'Admin')
  const isSubscriber = computed(() => user.value?.userType === 'Subscriber')
  const userType = computed(() => user.value?.userType ?? null)
  const fullName = computed(() => user.value?.fullName ?? '')

  function setUser(data: UserData) {
    user.value = data
    sessionChecked.value = true
  }

  function clearUser() {
    user.value = null
    sessionChecked.value = false
    // Clear persisted state
    sessionStorage.removeItem('pinia-auth')
  }

  async function login(username: string, password: string, rememberMe = false): Promise<UserType> {
    isLoading.value = true
    try {
      const payload = {
        username,
        password,
        rememberMe,
        deviceName: navigator.userAgent,
        operatingSystem: navigator.platform,
      }

      // Try admin login first
      try {
        const res = await http.post<UserData>('/auth/admin/login', payload)
        setUser({
          userType: res.data.userType || 'Admin',
          fullName: res.data.fullName,
          role: res.data.role,
          hasActiveSubscription: res.data.hasActiveSubscription,
        })
        return 'Admin'
      } catch {
        // Try subscriber login
        const res = await http.post<UserData>('/auth/subscriber/login', payload)
        setUser({
          userType: res.data.userType || 'Subscriber',
          fullName: res.data.fullName,
          role: res.data.role,
          hasActiveSubscription: res.data.hasActiveSubscription,
        })
        return 'Subscriber'
      }
    } finally {
      isLoading.value = false
    }
  }

  async function logout() {
    try {
      await http.post('/auth/logout', {})
    } catch {
      // Ignore logout API errors
    } finally {
      clearUser()
    }
  }

  async function checkSession(): Promise<boolean> {
    // If we already have user data from persistence, trust it
    // The JWT cookie will be validated by the backend on actual API calls
    if (user.value !== null) {
      sessionChecked.value = true
      return true
    }

    // Try to restore from API only if no persisted state
    try {
      isLoading.value = true
      const res = await http.get<UserData>('/auth/me')
      if (res.data) {
        setUser(res.data)
        return true
      }
      return false
    } catch {
      // Don't clear user here - let the 401 interceptor handle it
      // This prevents logout on temporary network issues
      return false
    } finally {
      isLoading.value = false
      sessionChecked.value = true
    }
  }

  return {
    user,
    isLoading,
    sessionChecked,
    isAuthenticated,
    isAdmin,
    isSubscriber,
    userType,
    fullName,
    login,
    logout,
    checkSession,
    setUser,
    clearUser,
  }
})
