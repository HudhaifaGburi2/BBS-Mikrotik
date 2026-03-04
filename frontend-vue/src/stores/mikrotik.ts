import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiPost } from '@/services/http'

interface PppUser {
  id: string
  name: string
  password: string
  profile: string
  service: string
  disabled: boolean
  remoteAddress: string | null
  localAddress: string | null
}

interface PppProfile {
  id: string
  name: string
  localAddress: string | null
  remoteAddress: string | null
  rateLimit: string | null
  onlyOne: boolean
}

interface MikroTikResponse<T> {
  success: boolean
  data: T
  message: string
  error?: string
}

export const useMikroTikStore = defineStore('mikrotik', () => {
  // Connection state
  const isConnected = ref(false)
  const isLoading = ref(false)
  const errorMessage = ref('')
  
  // Data
  const pppUsers = ref<PppUser[]>([])
  const pppProfiles = ref<PppProfile[]>([])
  
  // Last fetch timestamps
  const lastUsersFetch = ref<Date | null>(null)
  const lastProfilesFetch = ref<Date | null>(null)

  async function testConnection(): Promise<boolean> {
    isLoading.value = true
    errorMessage.value = ''
    try {
      const res = await apiPost<MikroTikResponse<null>>('/mikrotik/test-connection', {})
      if (res.data?.success) {
        isConnected.value = true
        return true
      } else {
        errorMessage.value = res.data?.message || 'فشل الاتصال'
        isConnected.value = false
        return false
      }
    } catch (err: any) {
      errorMessage.value = err.response?.data?.message || 'فشل الاتصال بجهاز MikroTik'
      isConnected.value = false
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function fetchPppUsers(force = false): Promise<boolean> {
    // Skip if recently fetched (within 30 seconds) unless forced
    if (!force && lastUsersFetch.value && (Date.now() - lastUsersFetch.value.getTime()) < 30000) {
      return true
    }
    
    isLoading.value = true
    errorMessage.value = ''
    try {
      const res = await apiPost<MikroTikResponse<PppUser[]>>('/mikrotik/ppp-users', {})
      if (res.data?.success && res.data.data) {
        pppUsers.value = res.data.data
        isConnected.value = true
        lastUsersFetch.value = new Date()
        return true
      } else {
        errorMessage.value = res.data?.message || 'فشل جلب المستخدمين'
        return false
      }
    } catch (err: any) {
      errorMessage.value = err.response?.data?.message || 'فشل الاتصال بجهاز MikroTik'
      isConnected.value = false
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function fetchPppProfiles(force = false): Promise<boolean> {
    // Skip if recently fetched (within 30 seconds) unless forced
    if (!force && lastProfilesFetch.value && (Date.now() - lastProfilesFetch.value.getTime()) < 30000) {
      return true
    }
    
    isLoading.value = true
    errorMessage.value = ''
    try {
      const res = await apiPost<MikroTikResponse<PppProfile[]>>('/mikrotik/ppp-profiles', {})
      if (res.data?.success && res.data.data) {
        pppProfiles.value = res.data.data
        isConnected.value = true
        lastProfilesFetch.value = new Date()
        return true
      } else {
        errorMessage.value = res.data?.message || 'فشل جلب البروفايلات'
        return false
      }
    } catch (err: any) {
      errorMessage.value = err.response?.data?.message || 'فشل الاتصال بجهاز MikroTik'
      isConnected.value = false
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function addPppUser(username: string, password: string, profile: string): Promise<boolean> {
    try {
      const res = await apiPost<MikroTikResponse<PppUser>>('/mikrotik/ppp-users/add', {
        pppUsername: username,
        pppPassword: password,
        profile
      })
      if (res.data?.success) {
        await fetchPppUsers(true)
        return true
      }
      return false
    } catch {
      return false
    }
  }

  async function deletePppUser(username: string): Promise<boolean> {
    try {
      const res = await apiPost<MikroTikResponse<null>>('/mikrotik/ppp-users/delete', { pppUsername: username })
      if (res.data?.success) {
        await fetchPppUsers(true)
        return true
      }
      return false
    } catch {
      return false
    }
  }

  async function addPppProfile(name: string, rateLimit: string, localAddress?: string, remoteAddress?: string): Promise<boolean> {
    try {
      const res = await apiPost<MikroTikResponse<PppProfile>>('/mikrotik/ppp-profiles/add', {
        profileName: name,
        rateLimit,
        localAddress: localAddress || null,
        remoteAddress: remoteAddress || null
      })
      if (res.data?.success) {
        await fetchPppProfiles(true)
        return true
      }
      return false
    } catch {
      return false
    }
  }

  async function updatePppProfile(name: string, rateLimit: string): Promise<boolean> {
    try {
      const res = await apiPost<MikroTikResponse<null>>('/mikrotik/ppp-profiles/update', {
        profileName: name,
        rateLimit
      })
      if (res.data?.success) {
        await fetchPppProfiles(true)
        return true
      }
      return false
    } catch {
      return false
    }
  }

  async function deletePppProfile(name: string): Promise<boolean> {
    try {
      const res = await apiPost<MikroTikResponse<null>>('/mikrotik/ppp-profiles/delete', { profileName: name })
      if (res.data?.success) {
        await fetchPppProfiles(true)
        return true
      }
      return false
    } catch {
      return false
    }
  }

  function clearCache() {
    lastUsersFetch.value = null
    lastProfilesFetch.value = null
  }

  return {
    // State
    isConnected,
    isLoading,
    errorMessage,
    pppUsers,
    pppProfiles,
    
    // Actions
    testConnection,
    fetchPppUsers,
    fetchPppProfiles,
    addPppUser,
    deletePppUser,
    addPppProfile,
    updatePppProfile,
    deletePppProfile,
    clearCache
  }
})
