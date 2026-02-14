import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiGet, apiPost } from '@/services/http'
import type { Invoice, GenerateInvoiceCommand, ApiResponse } from '@/types'

export const useInvoicesStore = defineStore('invoices', () => {
  const invoices = ref<Invoice[]>([])
  const currentInvoice = ref<Invoice | null>(null)
  const isLoading = ref(false)

  async function fetchAll() {
    isLoading.value = true
    try {
      const res = await apiGet<Invoice[]>('/invoices')
      if (res.data) {
        invoices.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchById(id: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Invoice>(`/invoices/${id}`)
      if (res.data) {
        currentInvoice.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchBySubscriber(subscriberId: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Invoice[]>(`/invoices/subscriber/${subscriberId}`)
      if (res.data) {
        invoices.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchOverdue() {
    isLoading.value = true
    try {
      const res = await apiGet<Invoice[]>('/invoices/overdue')
      if (res.data) {
        invoices.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function generate(command: GenerateInvoiceCommand): Promise<ApiResponse<Invoice>> {
    return await apiPost<Invoice>('/invoices', command)
  }

  return {
    invoices,
    currentInvoice,
    isLoading,
    fetchAll,
    fetchById,
    fetchBySubscriber,
    fetchOverdue,
    generate,
  }
})
