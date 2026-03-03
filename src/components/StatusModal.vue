<template>
  <div v-if="visible" class="fixed inset-0 bg-gray-900 bg-opacity-70 flex items-center justify-center z-50">
    <div class="bg-gray-800 rounded-lg p-6 w-96">
      <p class="text-xl font-bold mb-4" v-html="title"></p>
      <div class="space-y-4">
        <p class="text-gray-300" v-html="message"></p>
        <div class="flex gap-2">
          <button @click="close" class="flex-1 bg-gray-700 hover:bg-gray-600 rounded px-4 py-2 transition-colors">
            关闭
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: '提示'
  },
  message: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['close'])

const close = () => {
  emit('close')
}
</script>