<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">日志信息</h1>

    <!-- 日志列表 -->
    <div class="bg-gray-800 rounded-lg p-6">
      <div class="flex justify-between items-center mb-4">
        <h2 class="text-xl font-bold">系统日志</h2>
        <button @click="loadLogs" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded transition-colors">
          刷新日志
        </button>
      </div>

      <div class="space-y-2 max-h-[600px] overflow-y-auto">
        <div v-for="(log, index) in logs" :key="index" 
             class="p-3 bg-gray-700 rounded font-mono text-sm"
             :class="getLogLevelClass(log)">
          {{ log }}
        </div>
        <div v-if="logs.length === 0" class="text-gray-400 text-center py-8">
          暂无日志信息
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getLogs } from '../services/api'

const logs = ref([])

// 获取日志级别对应的样式类
const getLogLevelClass = (log) => {
  if (log.includes('[Information]')) {
    return 'text-yellow-400'
  } else if (log.includes('[Warning]')) {
    return 'text-yellow-600'
  } else if (log.includes('[Error]')) {
    return 'text-red-500'
  } else if (log.includes('[Fatal]')) {
    return 'text-red-700'
  } else if (log.includes('[Debug]')) {
    return 'text-cyan-400'
  }
  return 'text-white'
}

// 加载日志
const loadLogs = async () => {
  try {
    const response = await getLogs()
    if (response.data.code === 1) {
      logs.value = response.data.data || []
    }
  } catch (error) {
    console.error('Failed to load logs:', error)
  }
}

onMounted(() => {
  loadLogs()
})
</script>

<style scoped>
.font-mono {
  font-family: 'Courier New', Courier, monospace;
}
</style>
