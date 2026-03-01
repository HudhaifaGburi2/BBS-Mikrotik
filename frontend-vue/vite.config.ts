import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 8080,
    proxy: {
      '/api': {
        target: 'http://localhost:5286',
        changeOrigin: true,
        secure: false,
        ws: true,
        // Rewrite cookies to work with the proxy
        cookieDomainRewrite: {
          '*': ''  // Remove domain from all cookies so they work on localhost
        },
        cookiePathRewrite: {
          '*': '/'  // Ensure all cookies have path=/
        },
        configure: (proxy) => {
          // Log proxy requests for debugging
          proxy.on('proxyReq', (_proxyReq, req) => {
            console.log(`[Proxy] ${req.method} ${req.url}`);
            if (req.headers.cookie) {
              console.log(`[Proxy] Forwarding cookies: ${req.headers.cookie.substring(0, 50)}...`);
            }
          });
          // Log proxy responses for debugging
          proxy.on('proxyRes', (proxyRes, _req) => {
            const setCookie = proxyRes.headers['set-cookie'];
            if (setCookie) {
              console.log(`[Proxy] Set-Cookie from backend: ${JSON.stringify(setCookie).substring(0, 100)}...`);
            }
          });
        },
      },
    },
  },
})
