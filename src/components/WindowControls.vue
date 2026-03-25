<template>
  <div v-if="showWindowControls" class="fixed top-4 right-4 z-50 flex gap-2">
    <button @click="minimizeWindow"
      class="w-4 h-4 rounded-full bg-green-500 hover:bg-green-600 transition-colors flex items-center justify-center">
      <span class="opacity-0 hover:opacity-100 text-white font-bold text-xs">-</span>
    </button>
    <button @click="closeWindow"
      class="w-4 h-4 rounded-full bg-red-500 hover:bg-red-600 transition-colors flex items-center justify-center">
      <span class="opacity-0 hover:opacity-100 text-white font-bold text-xs">×</span>
    </button>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'
import { sendMessage } from '../tools'

const props = defineProps({
  showWindowControls: {
    type: Boolean,
    default: true
  }
})

// 最小化窗口
const minimizeWindow = () => {
  try {
    sendMessage('window:minimize')
  } catch (error) {
    console.error('Failed to minimize window:', error)
  }
}

// 关闭窗口
const closeWindow = () => {
  try {
    sendMessage('window:close')
  } catch (error) {
    console.error('Failed to close window:', error)
  }
}

</script>