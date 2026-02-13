import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/nirvana/NirvanaLogin.vue')
  },
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue')
  },
  {
    path: '/game-accounts',
    name: 'GameAccounts',
    component: () => import('../views/game/GameAccounts.vue')
  },
  {
    path: '/servers',
    name: 'Servers',
    component: () => import('../views/game/netgame/Servers.vue')
  },
  {
    path: '/server/:id',
    name: 'ServerDetail',
    component: () => import('../views/game/netgame/ServerDetail.vue')
  },
  {
    path: '/skins',
    name: 'Skins',
    component: () => import('../views/game/skin/Skins.vue')
  },
  {
    path: '/skin/:id',
    name: 'SkinDetail',
    component: () => import('../views/game/skin/SkinDetail.vue')
  },
  {
    path: '/plugins',
    name: 'Plugins',
    component: () => import('../views/plugin/Plugins.vue')
  },
  {
    path: '/plugin-store',
    name: 'PluginStore',
    component: () => import('../views/plugin/PluginStore.vue')
  },
  {
    path: '/plugin/:id',
    name: 'PluginDetail',
    component: () => import('../views/plugin/PluginDetail.vue')
  },
  {
    path: '/proxy-manager',
    name: 'ProxyManager',
    component: () => import('../views/game/ProxyManager.vue')
  },
  {
    path: '/version',
    name: 'Version',
    component: () => import('../views/others/Version.vue')
  },
  {
    path: '/game-manager',
    name: 'GameManager',
    component: () => import('../views/game/GameLaunchManager.vue')
  },
  {
    path: '/game-rental/',
    name: 'GameRental',
    component: () => import('../views/game/rental/GameRental.vue')
  },
  {
    path: '/game-rental/:id',
    name: 'GameRentalDetail',
    component: () => import('../views/game/rental/GameRentalDetail.vue')
  },
  {
    path: '/user',
    name: 'UserHome',
    component: () => import('../views/nirvana/UserHome.vue')
  },
  {
    path: '/settings',
    name: 'Settings',
    component: () => import('../views/Settings.vue')
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router