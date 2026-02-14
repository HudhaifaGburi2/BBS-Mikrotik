import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ToastMessage, ToastType } from '@/types'

let nextId = 0

export const useToastStore = defineStore('toast', () => {
  const messages = ref<ToastMessage[]>([])

  function show(message: string, type: ToastType = 'info', duration = 3000) {
    const id = nextId++
    messages.value.push({ id, message, type })
    setTimeout(() => remove(id), duration)
  }

  function remove(id: number) {
    messages.value = messages.value.filter((m) => m.id !== id)
  }

  function success(message: string) { show(message, 'success') }
  function error(message: string) { show(message, 'error') }
  function warning(message: string) { show(message, 'warning') }
  function info(message: string) { show(message, 'info') }

  return { messages, show, remove, success, error, warning, info }
})
