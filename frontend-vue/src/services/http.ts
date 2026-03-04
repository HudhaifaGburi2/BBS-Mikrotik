import axios, { type AxiosInstance, type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import type { ApiResponse } from '@/types'

const baseURL = import.meta.env.VITE_API_BASE_URL || '/api'

const http: AxiosInstance = axios.create({
  baseURL,
  timeout: 30000,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (value?: unknown) => void
  reject: (reason?: unknown) => void
}> = []

function processQueue(error: unknown) {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve()
    }
  })
  failedQueue = []
}

http.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const csrfToken = getCsrfToken()
    if (csrfToken && config.method !== 'get') {
      config.headers.set('X-CSRF-TOKEN', csrfToken)
    }
    return config
  },
  (error) => Promise.reject(error)
)

http.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    if (error.response?.status === 401 && !originalRequest._retry) {
      // Don't retry auth endpoints - let the router guard handle auth flow
      if (originalRequest.url?.includes('/auth/')) {
        // Clear auth state for any 401 on auth endpoints (except login)
        if (!originalRequest.url?.includes('/login')) {
          const { useAuthStore } = await import('@/stores/auth')
          const authStore = useAuthStore()
          authStore.clearUser()
        }
        return Promise.reject(error)
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then(() => http(originalRequest)).catch(err => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        // Send empty request - refresh token is in HttpOnly cookie
        await http.post('/auth/refresh-token', {})
        processQueue(null)
        return http(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError)
        // Clear auth state completely
        const { useAuthStore } = await import('@/stores/auth')
        const authStore = useAuthStore()
        authStore.clearUser()
        // Use Vue Router for navigation instead of hard reload
        const { default: router } = await import('@/router')
        const currentPath = window.location.pathname
        // Only redirect if not already on login page
        if (currentPath !== '/' && !currentPath.includes('/login')) {
          router.push({ name: 'login', query: { redirect: currentPath } })
        }
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  }
)

function getCsrfToken(): string | null {
  const match = document.cookie.match(/XSRF-TOKEN=([^;]+)/)
  return match?.[1] ? decodeURIComponent(match[1]) : null
}

export async function apiGet<T>(endpoint: string): Promise<ApiResponse<T>> {
  const response = await http.get<T>(endpoint)
  return { success: true, message: '', data: response.data, errors: [] }
}

export async function apiPost<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
  const response = await http.post<T>(endpoint, data)
  return { success: true, message: '', data: response.data, errors: [] }
}

export async function apiPut<T>(endpoint: string, data?: unknown): Promise<ApiResponse<T>> {
  const response = await http.put<T>(endpoint, data)
  return { success: true, message: '', data: response.data, errors: [] }
}

export async function apiDelete<T>(endpoint: string): Promise<ApiResponse<T>> {
  const response = await http.delete<T>(endpoint)
  return { success: true, message: '', data: response.data, errors: [] }
}

export default http
