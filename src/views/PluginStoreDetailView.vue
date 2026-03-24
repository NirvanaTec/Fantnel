<template>
  <div class="p-6">
    <button @click="router.back()" class="mb-6 text-blue-400 hover:underline">
      ← 返回列表
    </button>

    <div v-if="pluginDetail" class="space-y-6">
      <div class="flex justify-between items-start">
        <div>
          <h1 class="text-2xl font-bold mb-2">{{ pluginDetail.name }}</h1>
          <p class="text-gray-400">作者: {{ pluginDetail.publisher }} | 发布时间: {{ pluginDetail.publishDate }}</p>
        </div>

        <!-- 安装按钮 -->
        <button @click="showInstallModal = true"
          class="bg-blue-500 hover:bg-blue-600 rounded px-8 py-3 transition-colors text-lg font-medium">
          安装插件
        </button>
      </div>

      <!-- 插件信息 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">插件信息</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="text-gray-400">版本:</span>
            <span class="ml-2">{{ pluginDetail.version }}</span>
          </div>
          <div>
            <span class="text-gray-400">下载量:</span>
            <span class="ml-2">{{ pluginDetail.downloadCount }}</span>
          </div>
        </div>
        <div class="mt-4">
          <h3 class="font-medium mb-2">插件描述</h3>
          <div v-html="pluginDetail.detailDescription" class="text-gray-300"></div>
        </div>

        <!-- 依赖 -->
        <div v-if="pluginDetail.dependencies && pluginDetail.dependencies.length > 0" class="mt-4">
          <h3 class="font-medium mb-2">依赖</h3>
          <ul class="list-disc list-inside text-gray-300">
            <li v-for="dep in pluginDetail.dependencies" :key="dep.id">
              <router-link :to="`/plugin-store/${dep.id}`" class="text-blue-400 hover:underline">
                {{ dep.name }}
              </router-link>
            </li>
          </ul>
        </div>
      </div>

      <!-- 安装确认弹窗 -->
      <StatusModal1 :visible="showInstallModal" :message="confirmMessage" @close="showInstallModal = false"
        @confirm="installPlugin" />

      <!-- 安装错误弹窗 -->
      <StatusModal :visible="showInstallErrorModal" :title="'信息'" :message="installError"
        @close="showInstallErrorModal = false" />

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getPluginDetail, installPlugin as apiInstallPlugin } from '../services/api'

import StatusModal from '../components/StatusModal.vue'
import StatusModal1 from '../components/StatusModal1.vue'

const route = useRoute()
const router = useRouter()
const pluginId = ref(route.params.id)

const showInstallModal = ref(false)

const installError = ref('')
const showInstallErrorModal = ref(false)

const pluginDetail = ref(null)

// 初始化
onMounted(() => {
  loadPluginDetail()
})

// 弹窗消息相关
const confirmMessage = computed(() => {
  let dependenciesHtml = '';
  if (pluginDetail.value.dependencies && pluginDetail.value.dependencies.length > 0) {
    dependenciesHtml = pluginDetail.value.dependencies.map(dep => `<a href="/plugin-store/${dep.id}" class="dependency-link" style="color: #2196F3; text-decoration: none; ">${dep.name}</a>`);
    return `<p>即将安装 ${pluginDetail.value.name} 插件</p><p>并安装 ${dependenciesHtml} 依赖，确认进行安装吗？</p>`;
  } else {
    dependenciesHtml = '无';
    return `<p>即将安装 ${pluginDetail.value.name} 插件</p><p>确认进行安装吗？</p>`;
  }
});

// 监听路由参数变化
watch(() => route.params.id, (newId) => {
  pluginId.value = newId
  loadPluginDetail()
})

// 加载插件详情
const loadPluginDetail = async () => {
  try {
    const response = await getPluginDetail(pluginId.value)
    if (response.data.code === 1) {
      pluginDetail.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load plugin detail:', error)
  }
}

// 安装插件
const installPlugin = async () => {
  try {
    const response = await apiInstallPlugin(pluginId.value)
    installError.value = response.data.msg;
    showInstallModal.value = false
    showInstallErrorModal.value = true
  } catch (error) {
    console.error('Failed to install plugin:', error)
    installError.value = error.message
    showInstallErrorModal.value = true
    showInstallModal.value = false
  }
}
</script>
