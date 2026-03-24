<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">插件管理</h1>

    <!-- 插件列表 -->
    <div class="bg-gray-800 rounded-lg overflow-hidden">
      <table class="w-full">
        <thead>
          <tr class="border-b border-gray-700">
            <th class="py-3 px-4 text-left">插件名称</th>
            <th class="py-3 px-4 text-left">版本</th>
            <th class="py-3 px-4 text-left">作者</th>
            <th class="py-3 px-4 text-left">状态</th>
            <th class="py-3 px-4 text-left">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="plugin in plugins" :key="plugin.id" class="border-b border-gray-700 hover:bg-gray-750">
            <td class="py-3 px-4">{{ plugin.name }}</td>
            <td class="py-3 px-4">{{ plugin.version }}</td>
            <td class="py-3 px-4">{{ plugin.author }}</td>
            <td class="py-3 px-4">
              <span :class="plugin.status === '1' ? 'text-green-400' : 'text-red-400'">
                {{ plugin.status === '1' ? '已开启' : '未开启' }}
              </span>
            </td>
            <td class="py-3 px-4">
              <div class="flex gap-2">
                <button @click="togglePlugin(plugin.id)" class="text-blue-400 hover:text-blue-300">
                  {{ plugin.status === '1' ? '关闭' : '开启' }}
                </button>
                <button @click="confirmDeletePlugin(plugin.id)" class="text-red-400 hover:text-red-300">
                  删除
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

     <!-- 确认删除 -->
    <StatusModal1 :visible="showPluginsModal" :message="'确认删除插件吗？'" @close="showPluginsModal = false" @confirm="deletePlugin" />

  </div>

</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getInstalledPlugins, togglePluginStatus, deletePlugin as apiDeletePlugin } from '../services/api'
import StatusModal1 from '../components/StatusModal1.vue'

const plugins = ref([])
const showPluginsModal = ref(false)
const deletePluginId = ref(-1);

// 初始化
onMounted(() => {
  loadPlugins()
})

// 加载插件列表
const loadPlugins = async () => {
  try {
    const response = await getInstalledPlugins()
    if (response.data.code === 1) {
      plugins.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load plugins:', error)
  }
}

// 切换插件状态
const togglePlugin = async (id) => {
  try {
    const response = await togglePluginStatus(id)
    if (response.data.code === 1) {
      await loadPlugins()
    }
  } catch (error) {
    console.error('Failed to toggle plugin:', error)
  }
}

// 删除插件
const confirmDeletePlugin = async (id) => {
  deletePluginId.value = id;
  showPluginsModal.value = true;
}

const deletePlugin = async () => {
  try {
    const response = await apiDeletePlugin(deletePluginId.value)
    if (response.data.code === 1) {
      await loadPlugins()
    }
  } catch (error) {
    console.error('Failed to delete plugin:', error)
  } finally {
    showPluginsModal.value = false;
  }
}

</script>

<style scoped>
.bg-gray-750 {
  background-color: rgba(59, 130, 246, 0.1);
}
</style>
