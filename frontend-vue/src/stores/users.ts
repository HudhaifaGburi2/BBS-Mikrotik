import { ref } from 'vue'
import { defineStore } from 'pinia'
import { apiGet, apiPost, apiPut, apiDelete } from '@/services/http'
import type {
  SystemUser,
  CreateSystemUserCommand,
  UpdateSystemUserCommand,
  ResetPasswordCommand,
  PagedResult,
} from '@/types'

export const useUsersStore = defineStore('users', () => {
  const users = ref<SystemUser[]>([])
  const currentUser = ref<SystemUser | null>(null)
  const isLoading = ref(false)
  const currentPage = ref(1)
  const totalPages = ref(1)

  async function fetchAll(page = 1, pageSize = 20, search = '', role = '') {
    isLoading.value = true
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
      if (search) params.append('search', search)
      if (role) params.append('role', role)
      const res = await apiGet<PagedResult<SystemUser>>(`/users?${params}`)
      if (res.data) {
        users.value = res.data.items
        currentPage.value = res.data.page
        totalPages.value = res.data.totalPages
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchById(id: string) {
    isLoading.value = true
    try {
      const res = await apiGet<SystemUser>(`/users/${id}`)
      if (res.data) {
        currentUser.value = res.data
      }
    } finally {
      isLoading.value = false
    }
  }

  async function createUser(cmd: CreateSystemUserCommand) {
    const res = await apiPost<SystemUser>('/users', cmd)
    if (res.data) {
      users.value.unshift(res.data)
    }
    return res.data
  }

  async function updateUser(id: string, cmd: UpdateSystemUserCommand) {
    const res = await apiPut<SystemUser>(`/users/${id}`, cmd)
    if (res.data) {
      const idx = users.value.findIndex((u) => u.id === id)
      if (idx !== -1) users.value[idx] = res.data
      currentUser.value = res.data
    }
  }

  async function resetPassword(id: string, cmd: ResetPasswordCommand) {
    await apiPut(`/users/${id}/reset-password`, cmd)
  }

  async function toggleActive(id: string) {
    const user = users.value.find((u) => u.id === id)
    if (!user) return
    await apiPut(`/users/${id}`, {
      email: user.email,
      fullName: user.fullName,
      role: user.role,
      isActive: !user.isActive,
    })
    user.isActive = !user.isActive
  }

  async function deleteUser(id: string) {
    await apiDelete(`/users/${id}`)
    users.value = users.value.filter((u) => u.id !== id)
  }

  return {
    users,
    currentUser,
    isLoading,
    currentPage,
    totalPages,
    fetchAll,
    fetchById,
    createUser,
    updateUser,
    resetPassword,
    toggleActive,
    deleteUser,
  }
})
