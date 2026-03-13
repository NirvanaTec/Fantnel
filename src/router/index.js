import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue')
    },
    {
      path: '/game-account',
      name: 'gameAccount',
      component: () => import('../views/GameAccountView.vue')
    },
    {
      path: '/network-server',
      name: 'networkServer',
      component: () => import('../views/NetworkServerView.vue')
    },
    {
      path: '/network-server/:id',
      name: 'networkServerDetail',
      component: () => import('../views/NetworkServerDetailView.vue')
    },
    {
      path: '/rental-server',
      name: 'rentalServer',
      component: () => import('../views/RentalServerView.vue')
    },
    {
      path: '/rental-server/:id',
      name: 'rentalServerDetail',
      component: () => import('../views/RentalServerDetailView.vue')
    },
    {
      path: '/my-skin',
      name: 'mySkin',
      component: () => import('../views/MySkinView.vue')
    },
    {
      path: '/my-skin/:id',
      name: 'skinDetail',
      component: () => import('../views/SkinDetailView.vue')
    },
    {
      path: '/plugin-management',
      name: 'pluginManagement',
      component: () => import('../views/PluginManagementView.vue')
    },
    {
      path: '/plugin-store',
      name: 'pluginStore',
      component: () => import('../views/PluginStoreView.vue')
    },
    {
      path: '/plugin-store/:id',
      name: 'pluginStoreDetail',
      component: () => import('../views/PluginStoreDetailView.vue')
    },
    {
      path: '/proxy-management',
      name: 'proxyManagement',
      component: () => import('../views/ProxyManagementView.vue')
    },
    {
      path: '/game-management',
      name: 'gameManagement',
      component: () => import('../views/GameManagementView.vue')
    },
    {
      path: '/system-settings',
      name: 'systemSettings',
      component: () => import('../views/SystemSettingsView.vue')
    },
    {
      path: '/nirvana-user',
      name: 'nirvanaUser',
      component: () => import('../views/NirvanaUserView.vue')
    },
    {
      path: '/logs',
      name: 'logs',
      component: () => import('../views/LogView.vue')
    }
  ]
})

export default router