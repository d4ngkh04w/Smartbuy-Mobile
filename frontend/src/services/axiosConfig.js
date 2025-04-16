import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'

const apiUrl = import.meta.env.VITE_API_URL || '/api'

const instance = axios.create({
  baseURL: apiUrl,
  timeout: 10000,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

instance.interceptors.request.use((config) => {
    const authStore = useAuthStore()
  
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
  
    return config
})

export default instance;
