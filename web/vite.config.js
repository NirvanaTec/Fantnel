import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/

export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:13521',
        changeOrigin: true
      }
    }
  },
  plugins: [
    vue(),
  ],
})
