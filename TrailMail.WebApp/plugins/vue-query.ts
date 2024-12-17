import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'

export default defineNuxtPlugin((nuxtApp) => {
    const queryClient = new QueryClient();
    nuxtApp.vueApp.use(VueQueryPlugin)
})