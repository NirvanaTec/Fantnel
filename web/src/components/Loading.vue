<template>
  <div v-if="visible" class="loading-overlay">
    <div class="loading-dialog">
      <div class="loading-spinner"></div>
      <p v-if="!error">{{ text }}</p>
      <p v-else>
        {{ text }}<br>
        <span v-if="countdown > 0" class="countdown-text">将在 {{ countdown }} 秒后返回...</span>
      </p>
    </div>
  </div>
</template>

<script setup>
import { watch } from 'vue'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  text: {
    type: String,
    default: '正在加载...'
  },
  error: {
    type: Boolean,
    default: false
  },
  countdown: {
    type: Number,
    default: 0
  }
})

// 监听countdown变化，实现倒计时跳转
watch(() => props.countdown, (newValue) => {
  if (newValue > 0) {
    const timer = setInterval(() => {
      // 倒计时逻辑在父组件中处理
    }, 1000)
    
    return () => clearInterval(timer)
  }
})
</script>

<style scoped>
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.loading-dialog {
  background-color: var(--sidebar-bg);
  color: var(--text-color);
  padding: 30px;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
  min-width: 300px;
  text-align: center;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid var(--border-color);
  border-top: 4px solid var(--sidebar-active);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

.loading-dialog p {
  margin: 0;
  font-size: 16px;
  color: var(--text-color);
  font-weight: 500;
}

.countdown-text {
  color: var(--text-color);
  opacity: 0.7;
  font-size: 14px;
}
</style>