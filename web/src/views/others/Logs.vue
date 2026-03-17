<script setup>
import { ref, onMounted } from 'vue'
import { getLogs } from '../../utils/Tools'

const logs = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    const response = await getLogs()
    if (response.code === 1) {
      logs.value = response.data
    }
  } catch (error) {
    console.error('获取日志失败:', error)
  } finally {
    loading.value = false
  }
})

function getLogLevel(log) {
  if (log.includes('[Information]')) return 'information'
  if (log.includes('[Warning]')) return 'warning'
  if (log.includes('[Error]')) return 'error'
  if (log.includes('[Fatal]')) return 'fatal'
  if (log.includes('[Debug]')) return 'debug'
  return 'default'
}
</script>

<template>
  <div class="logs-page">
    <h1>日志信息</h1>
    <div v-if="loading" class="loading">
      <p>加载中...</p>
    </div>
    <div v-else class="logs-container">
      <div v-if="logs.length === 0" class="no-logs">
        <p>暂无日志信息</p>
      </div>
      <div v-else class="logs-list">
        <div v-for="(log, index) in logs" :key="index" class="log-item" :class="getLogLevel(log)">
          <div class="log-content">{{ log }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.logs-page {
  padding: 20px;
}

.logs-page h1 {
  margin-bottom: 20px;
  color: var(--text-color);
}

.loading {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
  color: var(--text-color);
}

.no-logs {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
  color: var(--text-color);
  background-color: var(--ad-bg);
  border-radius: 8px;
  border: 1px solid var(--border-color);
}

.logs-container {
  background-color: var(--ad-bg);
  border-radius: 8px;
  border: 1px solid var(--border-color);
  padding: 20px;
  max-height: 600px;
  overflow-y: auto;
}

.logs-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.log-item {
  padding: 10px;
  background-color: var(--bg-color);
  border-radius: 4px;
  border: 1px solid var(--border-color);
  font-family: monospace;
  white-space: pre-wrap;
  word-break: break-all;
}

.log-content {
  font-size: 14px;
  line-height: 1.5;
}

/* 日志级别颜色 */
.log-item.information .log-content {
  color: #ffcc00; /* Yellow */
}

.log-item.warning .log-content {
  color: #cc9900; /* DarkYellow */
}

.log-item.error .log-content {
  color: #ff0000; /* Red */
}

.log-item.fatal .log-content {
  color: #cc0000; /* DarkRed */
}

.log-item.debug .log-content {
  color: #00ffff; /* Cyan */
}

.log-item.default .log-content {
  color: var(--text-color);
}
</style>