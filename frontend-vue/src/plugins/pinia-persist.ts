import type { PiniaPluginContext } from 'pinia'

export function createPersistedState() {
  return (context: PiniaPluginContext) => {
    const { store } = context
    
    // Only persist auth store
    if (store.$id !== 'auth') return

    // Restore state from sessionStorage on initialization
    const savedState = sessionStorage.getItem(`pinia-${store.$id}`)
    if (savedState) {
      try {
        store.$patch(JSON.parse(savedState))
      } catch (e) {
        console.error('Failed to restore persisted state:', e)
      }
    }

    // Save state to sessionStorage on every mutation
    store.$subscribe((_mutation, state) => {
      try {
        sessionStorage.setItem(`pinia-${store.$id}`, JSON.stringify(state))
      } catch (e) {
        console.error('Failed to persist state:', e)
      }
    })
  }
}
