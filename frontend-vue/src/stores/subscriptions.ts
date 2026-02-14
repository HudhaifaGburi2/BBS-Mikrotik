import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiGet, apiPost } from '@/services/http'
import type { Subscription, CreateSubscriptionCommand, ApiResponse } from '@/types'

export const useSubscriptionsStore = defineStore('subscriptions', () => {
  const subscriptions = ref<Subscription[]>([])
  const currentSubscription = ref<Subscription | null>(null)
  const isLoading = ref(false)

  async function fetchAll() {
    isLoading.value = true
    try {
      const res = await apiGet<Subscription[]>('/subscriptions')
      if (res.data) {
        subscriptions.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchById(id: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Subscription>(`/subscriptions/${id}`)
      if (res.data) {
        currentSubscription.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function create(command: CreateSubscriptionCommand): Promise<ApiResponse<Subscription>> {
    return await apiPost<Subscription>('/subscriptions', command)
  }

  async function renew(id: string): Promise<ApiResponse<Subscription>> {
    return await apiPost<Subscription>(`/subscriptions/${id}/renew`)
  }

  async function suspend(id: string): Promise<ApiResponse<Subscription>> {
    return await apiPost<Subscription>(`/subscriptions/${id}/suspend`, {})
  }

  async function cancel(id: string, reason: string): Promise<ApiResponse<Subscription>> {
    return await apiPost<Subscription>(`/subscriptions/${id}/cancel`, { reason })
  }

  return {
    subscriptions,
    currentSubscription,
    isLoading,
    fetchAll,
    fetchById,
    create,
    renew,
    suspend,
    cancel,
  }
})
