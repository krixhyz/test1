import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: false,
  },
  server: {
    middlewareMode: false,
    fs: {
      strict: false,
      allow: ['..']
    },
    hmr: {
      overlay: false
    }
  },
  optimizeDeps: {
    force: true
  }
})
