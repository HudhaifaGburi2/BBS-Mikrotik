import { defineStore } from 'pinia'
import { ref } from 'vue'
import { apiGet, apiPost, apiPut, apiDelete } from '@/services/http'
import type { Plan, ApiResponse, CreatePlanCommand, UpdatePlanDto } from '@/types'

export const usePlansStore = defineStore('plans', () => {
  const plans = ref<Plan[]>([])
  const currentPlan = ref<Plan | null>(null)
  const isLoading = ref(false)

  async function fetchPlans(activeOnly = true) {
    isLoading.value = true
    try {
      const res = await apiGet<Plan[]>(`/plans?activeOnly=${activeOnly}`)
      if (res.data) {
        plans.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchPlan(id: string) {
    isLoading.value = true
    try {
      const res = await apiGet<Plan>(`/plans/${id}`)
      if (res.data) {
        currentPlan.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function createPlan(command: CreatePlanCommand): Promise<ApiResponse<Plan>> {
    return await apiPost<Plan>('/plans', command)
  }

  async function updatePlan(id: string, dto: UpdatePlanDto): Promise<ApiResponse<Plan>> {
    return await apiPut<Plan>(`/plans/${id}`, dto)
  }

  async function deletePlan(id: string): Promise<ApiResponse<boolean>> {
    return await apiDelete<boolean>(`/plans/${id}`)
  }

  return {
    plans,
    currentPlan,
    isLoading,
    fetchPlans,
    fetchPlan,
    createPlan,
    updatePlan,
    deletePlan,
  }
})
