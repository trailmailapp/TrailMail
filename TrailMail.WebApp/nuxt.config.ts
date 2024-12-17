import Nora from '@primevue/themes/nora';

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    compatibilityDate: '2024-04-03',
    devtools: {enabled: true},
    future: {
        compatibilityVersion: 4
    },

    modules: [
        '@primevue/nuxt-module',
        '@nuxtjs/tailwindcss'
    ],

    primevue: {
        options: {
            theme: {
                preset: Nora
            }
        }
    },

    runtimeConfig: {
        public: { api: process.env.services__Api__https__0 }
    }
})
