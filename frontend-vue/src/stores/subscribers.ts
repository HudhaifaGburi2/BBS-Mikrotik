import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiGet, apiPost, apiPut, apiDelete } from '@/services/http'
import type { Subscriber, PagedResult, CreateSubscriberCommand, UpdateSubscriberCommand } from '@/types'

export const useSubscribersStore = defineStore('subscribers', () => {
  const subscribers = ref<Subscriber[]>([])
  const currentSubscriber = ref<Subscriber | null>(null)
  const totalPages = ref(1)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const isLoading = ref(false)

  async function fetchSubscribers(page = 1, pageSize = 20, search = '', isActive?: boolean) {
    isLoading.value = true
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      })
      if (search) params.set('search', search)
      if (isActive !== undefined) params.set('isActive', isActive.toString())

      const res = await apiGet<PagedResult<Subscriber>>(`/subscribers?${params}`)
      if (res.data) {
        subscribers.value = res.data.items
        totalPages.value = res.data.totalPages
        totalCount.value = res.data.totalCount
        currentPage.value = res.data.page
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchSubscriber(id: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Subscriber>(`/subscribers/${id}`)
      if (res.data) {
        currentSubscriber.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function createSubscriber(command: CreateSubscriberCommand) {
    const res = await apiPost<Subscriber>('/subscribers', command)
    return res.data
  }

  async function updateSubscriber(id: string, command: UpdateSubscriberCommand) {
    const res = await apiPut<Subscriber>(`/subscribers/${id}`, command)
    return res.data
  }

  async function suspendSubscriber(id: string) {
    await apiPost(`/subscribers/${id}/suspend`, {})
  }

  async function unsuspendSubscriber(id: string) {
    await apiPost(`/subscribers/${id}/unsuspend`, {})
  }

  async function deleteSubscriber(id: string, forceDelete = false) {
    await apiDelete(`/subscribers/${id}?forceDelete=${forceDelete}`)
  }

  async function resetPassword(id: string) {
    const res = await apiPost<string>(`/subscribers/${id}/reset-password`, {})
    return res.data
  }

  async function resetSystemPassword(id: string, newPassword: string) {
    const res = await apiPost<{ message: string; password: string }>(`/subscribers/${id}/reset-system-password`, { newPassword })
    return res.data
  }

  async function resetMikroTikPassword(id: string, newPassword: string) {
    const res = await apiPost<{ message: string; password: string }>(`/subscribers/${id}/reset-mikrotik-password`, { newPassword })
    return res.data
  }

  async function changePlan(id: string, planId: string) {
    const res = await apiPost(`/subscribers/${id}/change-plan`, { planId })
    return res.data
  }

  return {
    subscribers,
    currentSubscriber,
    totalPages,
    totalCount,
    currentPage,
    isLoading,
    fetchSubscribers,
    fetchSubscriber,
    createSubscriber,
    updateSubscriber,
    suspendSubscriber,
    unsuspendSubscriber,
    deleteSubscriber,
    resetPassword,
    resetSystemPassword,
    resetMikroTikPassword,
    changePlan,
  }
})
