<template>
  <div v-if="messages.length > 0" class="toast-container">
    <div 
      v-for="(msg, index) in messages" 
      :key="index"
      :class="['toast-message', `toast-${msg.type}`]"
      @transitionend="removeMessage(index)"
    >
      {{ msg.content }}
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const messages = ref([])
let messageId = 0

// 显示消息
const showMessage = (content, type = 'info', duration = 3000) => {
  const id = messageId++
  messages.value.push({
    id,
    content,
    type,
    duration
  })

  // 自动移除消息
  setTimeout(() => {
    const index = messages.value.findIndex(msg => msg.id === id)
    if (index > -1) {
      messages.value.splice(index, 1)
    }
  }, duration)
}

// 移除消息
const removeMessage = (index) => {
  // 过渡结束后移除消息
  if (!messages.value[index]) return
  messages.value.splice(index, 1)
}

// 导出消息方法
const Message = {
  success: (content, duration) => showMessage(content, 'success', duration),
  error: (content, duration) => showMessage(content, 'error', duration),
  info: (content, duration) => showMessage(content, 'info', duration),
  warning: (content, duration) => showMessage(content, 'warning', duration)
}

defineExpose({
  Message
})
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.toast-message {
  padding: 12px 20px;
  border-radius: 8px;
  color: white;
  font-size: 14px;
  font-weight: 500;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  opacity: 0;
  transform: translateY(-20px);
  animation: toastShow 0.3s ease forwards;
}

.toast-success {
  background-color: #4CAF50;
}

.toast-error {
  background-color: #F44336;
}

.toast-info {
  background-color: #2196F3;
}

.toast-warning {
  background-color: #FFC107;
  color: #333;
}

@keyframes toastShow {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>