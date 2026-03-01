import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './style.css'
import { createPersistedState } from './plugins/pinia-persist'

const app = createApp(App)

const pinia = createPinia()
pinia.use(createPersistedState())

app.use(pinia)
app.use(router)

app.config.errorHandler = (err, _instance, info) => {
  console.error('Global error:', err, info)
}

app.mount('#app')
