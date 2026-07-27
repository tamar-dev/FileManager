import base44 from "@base44/vite-plugin"
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import path from 'path'

const base44Enabled = process.env.BASE44_ENABLED === 'true'

export default defineConfig({
  plugins: [
    ...(base44Enabled
      ? [
          base44({
            legacySDKImports:
              process.env.BASE44_LEGACY_SDK_IMPORTS === 'true',
            hmrNotifier: true,
            navigationNotifier: true,
            analyticsTracker: true,
            visualEditAgent: true
          })
        ]
      : []),

    react(),
  ],

  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
})