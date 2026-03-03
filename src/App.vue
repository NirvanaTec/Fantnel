<template>
  <div class="flex h-screen overflow-hidden bg-gray-900 text-white">
    <!-- 侧边导航栏 -->
    <aside class="w-64 border-r border-gray-800 bg-gray-900 flex flex-col">
      <!-- Logo -->
      <div class="p-4 border-b border-gray-800">
        <h1 class="text-xl font-bold text-blue-400">Fantnel</h1>
      </div>
      
      <!-- 涅槃账号显示 -->
      <div class="p-4 border-b border-gray-800 cursor-pointer hover:bg-gray-800 transition-colors" @click="handleNirvanaAccountClick">
        <div v-if="nirvanaAccount" class="flex items-center gap-2">
          <span class="text-sm text-gray-400">涅槃账号:</span>
          <span class="font-medium">{{ nirvanaAccount.account }}</span>
        </div>
        <div v-else class="text-blue-400 hover:underline">点我登录涅槃账号</div>
      </div>
      
      <!-- 导航菜单 -->
      <nav class="flex-1 overflow-y-auto p-4">
        <ul class="space-y-6">
          <!-- 主页 -->
          <li>
            <router-link to="/" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              主页
            </router-link>
          </li>
          
          <!-- 游戏账号 -->
          <li>
            <div class="font-medium text-gray-400 mb-2">游戏账号</div>
            <router-link to="/game-account" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              游戏账号
            </router-link>
          </li>
          
          <!-- 服务器 -->
          <li>
            <div class="font-medium text-gray-400 mb-2">服务器</div>
            <router-link to="/network-server" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              网络服
            </router-link>
            <router-link to="/rental-server" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              租赁服
            </router-link>
          </li>
          
          <!-- 皮肤 -->
          <li>
            <router-link to="/my-skin" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              我的皮肤
            </router-link>
          </li>
          
          <!-- 插件 -->
          <li>
            <div class="font-medium text-gray-400 mb-2">插件</div>
            <router-link to="/plugin-management" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              插件管理
            </router-link>
            <router-link to="/plugin-store" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              插件商城
            </router-link>
          </li>
          
          <!-- 管理 -->
          <li>
            <div class="font-medium text-gray-400 mb-2">管理</div>
            <router-link to="/proxy-management" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              代理管理
            </router-link>
            <router-link to="/game-management" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              游戏管理
            </router-link>
          </li>
          
          <!-- 系统设置 -->
          <li>
            <router-link to="/system-settings" class="block py-2 px-3 rounded hover:bg-gray-800 transition-colors" active-class="bg-gray-800 text-blue-400">
              系统设置
            </router-link>
          </li>
          
          <!-- 主题切换 -->
          <li>
            <div class="font-medium text-gray-400 mb-2">主题切换</div>
            <div class="space-y-2">
              <button @click="setTheme('default')" class="w-full text-left py-2 px-3 rounded hover:bg-gray-800 transition-colors" :class="{ 'bg-gray-800 text-blue-400': currentTheme === 'default' }">
                默认主题
              </button>
              <!-- <button @click="setTheme('dark')" class="w-full text-left py-2 px-3 rounded hover:bg-gray-800 transition-colors" :class="{ 'bg-gray-800 text-blue-400': currentTheme === 'dark' }">
                深色主题
              </button> -->
            </div>
          </li>
        </ul>
      </nav>
    </aside>
    
    <!-- 主内容区 -->
    <main class="flex-1 overflow-y-auto bg-gray-900">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    
    <!-- 登录模态框 -->
    <div v-if="showLoginModal" class="fixed inset-0 bg-gray-900 bg-opacity-70 flex items-center justify-center z-50">
      <div class="bg-gray-800 rounded-lg p-6 w-96">
        <h2 class="text-xl font-bold mb-4">登录涅槃账号</h2>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-gray-400 mb-1">账号</label>
            <input type="text" v-model="loginForm.account" class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">密码</label>
            <input type="password" v-model="loginForm.password" class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div class="flex gap-2">
            <button @click="handleLogin" class="flex-1 bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
              登录
            </button>
            <button @click="showLoginModal = false" class="flex-1 bg-gray-700 hover:bg-gray-600 rounded px-4 py-2 transition-colors">
              取消
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getNirvanaAccountInfo, loginNirvanaAccount, getTheme, setTheme as apiSetTheme } from './services/api'

const router = useRouter()
const nirvanaAccount = ref(null)
const showLoginModal = ref(false)
const loginForm = ref({ account: '', password: '' })
const currentTheme = ref('default')

// 初始化
onMounted(() => {
  loadNirvanaAccount()
  loadCurrentTheme()
})

// 加载涅槃账号信息
const loadNirvanaAccount = async () => {
  try {
    const response = await getNirvanaAccountInfo()
    if (response.data.code === 1) {
      nirvanaAccount.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load nirvana account:', error)
  }
}

// 加载当前主题
const loadCurrentTheme = async () => {
  try {
    const response = await getTheme()
    if (response.data.code === 1) {
      currentTheme.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load theme:', error)
  }
}

// 处理涅槃账号点击
const handleNirvanaAccountClick = () => {
  if (nirvanaAccount.value) {
    router.push('/nirvana-user')
  } else {
    showLoginModal.value = true
  }
}

// 处理登录
const handleLogin = async () => {
  try {
    const response = await loginNirvanaAccount(loginForm.value.account, loginForm.value.password)
    if (response.data.code === 1) {
      await loadNirvanaAccount()
      showLoginModal.value = false
    }
  } catch (error) {
    console.error('Login failed:', error)
  }
}

// 设置主题
const setTheme = async (theme) => {
  try {
    const response = await apiSetTheme(theme)
    if (response.data.code === 1) {
      currentTheme.value = theme
    }
  } catch (error) {
    console.error('Failed to set theme:', error)
  }
}
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>