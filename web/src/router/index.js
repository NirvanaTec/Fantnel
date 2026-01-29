import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue')
  },
  {
    path: '/game-accounts',
    name: 'GameAccounts',
    component: () => import('../views/GameAccounts.vue')
  },
  {
    path: '/servers',
    name: 'Servers',
    component: () => import('../views/Servers.vue')
  },
  {
    path: '/server/:id',
    name: 'ServerDetail',
    component: () => import('../views/ServerDetail.vue')
  },
  {
    path: '/skins',
    name: 'Skins',
    component: () => import('../views/Skins.vue')
  },
  {
    path: '/skin/:id',
    name: 'SkinDetail',
    component: () => import('../views/SkinDetail.vue')
  },
  {
    path: '/plugins',
    name: 'Plugins',
    component: () => import('../views/Plugins.vue')
  },
  {
    path: '/plugin-store',
    name: 'PluginStore',
    component: () => import('../views/PluginStore.vue')
  },
  {
    path: '/plugin/:id',
    name: 'PluginDetail',
    component: () => import('../views/PluginDetail.vue')
  },
  {
    path: '/proxy-manager',
    name: 'ProxyManager',
    component: () => import('../views/ProxyManager.vue')
  },
  {
    path: '/version',
    name: 'Version',
    component: () => import('../views/Version.vue')
  },
  {
    path: '/game-manager',
    name: 'GameManager',
    component: () => import('../views/GameLaunchManager.vue')
  },
  {
    path: '/game-rental/',
    name: 'GameRental',
    component: () => import('../views/GameRental.vue')
  },
  {
    path: '/game-rental/:id',
    name: 'GameRentalDetail',
    component: () => import('../views/GameRentalDetail.vue')
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router