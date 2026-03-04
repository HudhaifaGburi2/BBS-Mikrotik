import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import http from '@/services/http'
import type { UserData, UserType } from '@/types'

const AUTH_STORAGE_KEY = 'Dushi_auth_user'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserData | null>(null)
  const isLoading = ref(false)
  const initialized = ref(false)
  const sessionValidated = ref(false)

  const isAuthenticated = computed(() => user.value !== null && sessionValidated.value)
  const isAdmin = computed(() => user.value?.userType === 'Admin' && sessionValidated.value)
  const isSubscriber = computed(() => user.value?.userType === 'Subscriber' && sessionValidated.value)
  const userType = computed(() => user.value?.userType ?? null)
  const fullName = computed(() => user.value?.fullName ?? '')

  // Initialize from localStorage on store creation (for UI display only, not auth)
  function initFromStorage() {
    if (initialized.value) return
    try {
      const stored = localStorage.getItem(AUTH_STORAGE_KEY)
      if (stored) {
        const parsed = JSON.parse(stored)
        if (parsed && parsed.userType) {
          user.value = parsed
          // Don't set sessionValidated - must be validated with backend
        }
      }
    } catch {
      localStorage.removeItem(AUTH_STORAGE_KEY)
    }
    initialized.value = true
  }

  // Call init immediately
  initFromStorage()

  function setUser(data: UserData, validated = true) {
    user.value = data
    sessionValidated.value = validated
    // Persist to localStorage
    try {
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(data))
    } catch {
      // Ignore storage errors
    }
  }

  function clearUser() {
    user.value = null
    sessionValidated.value = false
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
    // If session already validated in this browser session, skip re-validation
    if (sessionValidated.value && user.value !== null) {
      return true
    }

    // Always validate JWT with backend - don't trust localStorage alone
    try {
      isLoading.value = true
      const res = await http.get<UserData>('/auth/me')
      if (res.data) {
        setUser(res.data, true)
        return true
      }
      clearUser()
      return false
    } catch (err: unknown) {
      // Any error means session is invalid
      clearUser()
      return false
    } finally {
      isLoading.value = false
    }
  }

  // Force re-validation of session (e.g., after server restart)
  async function validateSession(): Promise<boolean> {
    sessionValidated.value = false
    return checkSession()
  }

  return {
    user,
    isLoading,
    initialized,
    sessionValidated,
    isAuthenticated,
    isAdmin,
    isSubscriber,
    userType,
    fullName,
    login,
    logout,
    checkSession,
    validateSession,
    setUser,
    clearUser,
    initFromStorage,
  }
})
