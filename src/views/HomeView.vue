<template>
  <div class="p-6" @drop="handleDrop" @dragover.prevent @dragenter.prevent>
    <h1 class="text-2xl font-bold mb-6">欢迎使用 Fantnel</h1>

    <!-- 广告区域 -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
      <div class="bg-gray-800 rounded-lg p-4">
        <h3 class="text-lg font-bold text-blue-400 mb-2">{{ homeInfo.ad1.name }}</h3>
        <p class="text-gray-300">{{ homeInfo.ad1.text }}</p>
      </div>
      <div class="bg-gray-800 rounded-lg p-4">
        <h3 class="text-lg font-bold text-blue-400 mb-2">{{ homeInfo.ad2.name }}</h3>
        <p class="text-gray-300">{{ homeInfo.ad2.text }}</p>
      </div>
      <div class="bg-gray-800 rounded-lg p-4">
        <h3 class="text-lg font-bold text-blue-400 mb-2">{{ homeInfo.ad3.name }}</h3>
        <p class="text-gray-300">{{ homeInfo.ad3.text }}</p>
      </div>
    </div>

    <!-- 拖拽区域 -->
    <div
      class="bg-gray-800 rounded-lg p-8 mb-8 border-2 border-dashed border-gray-600 hover:border-blue-400 transition-colors">
      <div class="text-center">
        <p class="text-gray-400 mb-2">拖拽 主题 文件到此处</p>
        <p class="text-sm text-gray-500 mb-2">或点击选择文件</p>
        <a href="http://npyyds.top/fantnel/theme" target="_blank">
          <p class="text-sm text-gray-500">下载主题，请前往 <b>涅槃科技</b> 下载</p>
        </a>
        <input type="file" accept=".fant.json" class="hidden" ref="fileInput" @change="handleFileSelect">
        <button class="mt-4 px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded" @click="$refs.fileInput.click()">
          选择 主题 文件
        </button>
      </div>
    </div>

    <!-- 系统信息 -->
    <div class="bg-gray-800 rounded-lg p-6">
      <h2 class="text-xl font-bold mb-4">系统信息</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="flex items-center gap-2">
          <span class="text-gray-400">游戏版本:</span>
          <span>{{ homeInfo.gameVersion }}</span>
        </div>
        <div class="flex items-center gap-2">
          <span class="text-gray-400">CRC Salt:</span>
          <span>{{ homeInfo.crcSalt }}</span>
        </div>
      </div>
    </div>

    <!-- 主题状态 -->
    <StatusModal :visible="showModal" :message="modalMessage" @close="reload()" />

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getHomeInfo, setThemeSwitch } from '../services/api'
import StatusModal from '../components/StatusModal.vue'

const showModal = ref(false)
const modalMessage = ref('')

const homeInfo = ref({
  ad1: {
    name: "涅槃科技",
    text: "涅槃科技"
  },
  ad2: {
    name: "涅槃科技",
    text: "涅槃科技"
  },
  ad3: {
    name: "涅槃科技",
    text: "涅槃科技"
  },
  crcSalt: "涅槃科技",
  gameVersion: "涅槃科技"
})

const handleDrop = (event) => {
  event.preventDefault()
  const files = event.dataTransfer.files
  if (files.length > 0) {
    processFile(files[0])
  }
}

const handleFileSelect = (event) => {
  const files = event.target.files
  if (files.length > 0) {
    processFile(files[0])
  }
}

const processFile = (file) => {
  if (file.name.endsWith('.fant.json')) {
    const reader = new FileReader()
    reader.onload = (e) => {
      try {
        showModal.value = true
        modalMessage.value = '正在应用主题，请稍后...'
        var json = JSON.parse(e.target.result)
        setThemeSwitch(json).then(response => {
          modalMessage.value = response.data.msg
        })
      } catch (error) {
        console.error('Fantnel 主题 解析错误:', error)
        showModal.value = true
        modalMessage.value = 'Fantnel 主题 文件解析失败，请检查文件格式。'
      }
    }
    reader.onerror = () => {
      showModal.value = true
      modalMessage.value = '文件读取失败，请重试。'
    }
    reader.readAsText(file)
  } else {
    showModal.value = true
    modalMessage.value = '请选择 Fantnel 主题 格式的文件。'
  }
}

const reload = () => {
  location.reload(true);
}

onMounted(async () => {
  try {
    const response = await getHomeInfo()
    homeInfo.value = response.data
  } catch (error) {
    console.error('Failed to load home info:', error)
  }
})
</script>
