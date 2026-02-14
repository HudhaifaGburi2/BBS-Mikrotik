import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiGet, apiPost } from '@/services/http'
import type { Payment, ProcessPaymentCommand, ApiResponse } from '@/types'

export const usePaymentsStore = defineStore('payments', () => {
  const payments = ref<Payment[]>([])
  const currentPayment = ref<Payment | null>(null)
  const isLoading = ref(false)

  async function fetchAll() {
    isLoading.value = true
    try {
      const res = await apiGet<Payment[]>('/payments')
      if (res.data) {
        payments.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchById(id: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Payment>(`/payments/${id}`)
      if (res.data) {
        currentPayment.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchByInvoice(invoiceId: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Payment[]>(`/payments/invoice/${invoiceId}`)
      if (res.data) {
        payments.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchBySubscriber(subscriberId: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Payment[]>(`/payments/subscriber/${subscriberId}`)
      if (res.data) {
        payments.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function processPayment(command: ProcessPaymentCommand): Promise<ApiResponse<Payment>> {
    return await apiPost<Payment>('/payments', command)
  }

  return {
    payments,
    currentPayment,
    isLoading,
    fetchAll,
    fetchById,
    fetchByInvoice,
    fetchBySubscriber,
    processPayment,
  }
})
