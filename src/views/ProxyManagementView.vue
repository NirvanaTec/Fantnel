<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">代理管理</h1>

    <!-- 代理列表 -->
    <div v-if="proxiesData" class="space-y-6">
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">代理信息</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <div>
            <span class="text-gray-400">代理IP:</span>
            <span class="ml-2">{{ proxiesData.ip }}</span>
          </div>
        </div>

        <!-- 代理实例 -->
        <div class="space-y-4" v-if="proxiesData.proxies.length > 0">
          <div v-for="proxy in proxiesData.proxies" :key="proxy.id" class="bg-gray-700 rounded-lg p-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <span class="text-gray-400">服务器名称:</span>
                <span class="ml-2">{{ proxy.Interceptor.ServerName }}</span>
              </div>
              <div>
                <span class="text-gray-400">服务器版本:</span>
                <span class="ml-2">{{ proxy.Interceptor.ServerVersion }}</span>
              </div>
              <div>
                <span class="text-gray-400">角色名称:</span>
                <span class="ml-2">{{ proxy.Interceptor.NickName }}</span>
              </div>
              <div>
                <span class="text-gray-400">代理端口:</span>
                <span class="ml-2">{{ proxy.Interceptor.LocalPort }}</span>
              </div>
              <div>
                <span class="text-gray-400">目标服务器:</span>
                <span class="ml-2">{{ proxy.Interceptor.ForwardAddress }}:{{ proxy.Interceptor.ForwardPort }}</span>
              </div>
              <div>
                <span class="text-gray-400">用户ID:</span>
                <span class="ml-2">{{ proxy.Interceptor.CurrentConfig.user_id }}</span>
              </div>
            </div>
            <div class="mt-4 flex gap-2">
              <button @click="copyProxyInfo(proxiesData.ip, proxy.Interceptor.LocalPort)"
                class="px-4 py-2 bg-blue-500 hover:bg-blue-600 rounded transition-colors">
                复制代理信息
              </button>
              <button @click="closeProxy(proxy.id)"
                class="px-4 py-2 bg-red-500 hover:bg-red-600 rounded transition-colors">
                关闭代理
              </button>
            </div>
          </div>
        </div>
        <div v-else class="bg-gray-800 rounded-lg p-6 text-center">
          <p class="text-gray-400">暂无启动的代理</p>
        </div>

      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getLaunchedProxies, closeProxy as apiCloseProxy } from '../services/api'

const proxiesData = ref(null)

// 初始化
onMounted(() => {
  loadProxies()
})

// 加载代理信息
const loadProxies = async () => {
  try {
    const response = await getLaunchedProxies()
    if (response.data.code === 1) {
      proxiesData.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load proxies:', error)
  }
}

// 关闭代理
const closeProxy = async (id) => {
  try {
    const response = await apiCloseProxy(id)
    if (response.data.code === 1) {
      await loadProxies()
    }
  } catch (error) {
    console.error('Failed to close proxy:', error)
  }
}

// 复制代理信息
const copyProxyInfo = (ip, port) => {
  const proxyInfo = `${ip}:${port}`
  navigator.clipboard.writeText(proxyInfo).then(() => {
    alert('代理信息已复制到剪贴板')
  }).catch(err => {
    console.error('Failed to copy:', err)
  })
}
</script>
