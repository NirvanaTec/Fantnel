<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">插件商城</h1>
    
    <!-- 插件列表 -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div v-for="plugin in plugins" :key="plugin.id" class="bg-gray-800 rounded-lg p-6 hover:shadow-lg transition-shadow">
        <h3 class="text-lg font-bold mb-2">{{ plugin.name }}</h3>
        <p class="text-gray-300 text-sm mb-4">{{ plugin.shortDescription }}</p>
        <div class="grid grid-cols-2 gap-2 mb-4">
          <div>
            <span class="text-gray-400 text-sm">作者:</span>
            <span class="block">{{ plugin.publisher }}</span>
          </div>
          <div>
            <span class="text-gray-400 text-sm">下载量:</span>
            <span class="block">{{ plugin.downloadCount }}</span>
          </div>
        </div>
        <router-link :to="`/plugin-store/${plugin.id}`" class="text-blue-400 hover:underline">
          查看详情
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getPluginStore } from '../services/api'

const plugins = ref([])

// 初始化
onMounted(() => {
  loadPlugins()
})

// 加载插件列表
const loadPlugins = async () => {
  try {
    const response = await getPluginStore()
    if (response.data.code === 1) {
      plugins.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load plugin store:', error)
  }
}
</script>
