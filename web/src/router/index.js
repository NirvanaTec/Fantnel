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
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router