import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import http from '@/services/http'
import type { UserData, UserType } from '@/types'

const AUTH_STORAGE_KEY = 'Dushi_auth_user'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserData | null>(null)
  const isLoading = ref(false)
  const initialized = ref(false)

  const isAuthenticated = computed(() => user.value !== null)
  const isAdmin = computed(() => user.value?.userType === 'Admin')
  const isSubscriber = computed(() => user.value?.userType === 'Subscriber')
  const userType = computed(() => user.value?.userType ?? null)
  const fullName = computed(() => user.value?.fullName ?? '')

  // Initialize from localStorage on store creation
  function initFromStorage() {
    if (initialized.value) return
    try {
      const stored = localStorage.getItem(AUTH_STORAGE_KEY)
      if (stored) {
        const parsed = JSON.parse(stored)
        if (parsed && parsed.userType) {
          user.value = parsed
        }
      }
    } catch {
      localStorage.removeItem(AUTH_STORAGE_KEY)
    }
    initialized.value = true
  }

  // Call init immediately
  initFromStorage()

  function setUser(data: UserData) {
    user.value = data
    // Persist to localStorage
    try {
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(data))
    } catch {
      // Ignore storage errors
    }
  }

  function clearUser() {
    user.value = null
    localStorage.removeItem(AUTH_STORAGE_KEY)
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
    // If we already have user data from localStorage, trust it
    // The JWT cookie will be validated by the backend on actual API calls
    if (user.value !== null) {
      return true
    }
    try {
      isLoading.value = true
      const res = await http.get<UserData>('/auth/me')
      if (res.data) {
        setUser(res.data)
        return true
      }
      return false
    } catch (err: any) {
      // Only clear if it's a 401 - other errors might be temporary
      if (err.response?.status === 401) {
        clearUser()
      }
      return false
    } finally {
      isLoading.value = false
    }
  }

  return {
    user,
    isLoading,
    initialized,
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
    initFromStorage,
  }
})
