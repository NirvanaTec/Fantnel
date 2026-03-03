<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">系统设置</h1>
    
    <!-- 系统设置 -->
    <div v-if="systemConfig" class="space-y-6">
      <!-- 主动登录设置 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">主动登录设置</h2>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <span>开启主动登录游戏账号</span>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" :checked="systemConfig.autoLoginGame" @change="updateConfig('autoLoginGame', !systemConfig.autoLoginGame)" class="sr-only peer">
              <div class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
            </label>
          </div>
          <div class="flex items-center justify-between">
            <span>开启主动登录 Cookie 游戏账号</span>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" :checked="systemConfig.autoLoginGameCookie" @change="updateConfig('autoLoginGameCookie', !systemConfig.autoLoginGameCookie)" class="sr-only peer">
              <div class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
            </label>
          </div>
           <div class="flex items-center justify-between">
            <span>开启主动登录 163Email 游戏账号</span>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" :checked="systemConfig.autoLoginGame163Email" @change="updateConfig('autoLoginGame163Email', !systemConfig.autoLoginGame163Email)" class="sr-only peer">
              <div class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
            </label>
          </div>
        </div>
      </div>
      
      <!-- 聊天室设置 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">聊天室设置</h2>
        <div class="space-y-4">
          <div class="flex items-center justify-between">
            <span>启用游戏聊天室</span>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" :checked="systemConfig.chatEnable" @change="updateConfig('chatEnable', !systemConfig.chatEnable)" class="sr-only peer">
              <div class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
            </label>
          </div>
          <div class="flex items-center justify-between">
            <span>聊天室玩家标明</span>
            <label class="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" :checked="systemConfig.chatTarget" @change="updateConfig('chatTarget', !systemConfig.chatTarget)" class="sr-only peer">
              <div class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
            </label>
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">启动代理-聊天室-玩家前缀</label>
            <input type="text" :value="systemConfig.chatPrefix" @change="updateConfig('chatPrefix', $event.target.value)" class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
        </div>
      </div>
      
      <!-- 启动配置 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">启动配置</h2>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-gray-400 mb-1">启动游戏最大内存 (MB)</label>
            <input type="number" :value="systemConfig.gameMemory" @change="updateConfig('gameMemory', parseInt($event.target.value))" class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">启动游戏-JVM参数</label>
            <input type="text" :value="systemConfig.jvmArgs" @change="updateConfig('jvmArgs', $event.target.value)" class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">启动游戏-游戏参数</label>
            <input type="text" :value="systemConfig.gameArgs" @change="updateConfig('gameArgs', $event.target.value)" class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getSystemConfig, setSystemConfig } from '../services/api'

const systemConfig = ref(null)

// 初始化
onMounted(() => {
  loadConfig()
})

// 加载配置
const loadConfig = async () => {
  try {
    const response = await getSystemConfig()
    if (response.data.code === 1) {
      systemConfig.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load config:', error)
  }
}

// 更新配置
const updateConfig = async (key, value) => {
  try {
    const response = await setSystemConfig(key, value)
    if (response.data.code === 1) {
      systemConfig.value[key] = value
    }
  } catch (error) {
    console.error('Failed to update config:', error)
  }
}
</script>
