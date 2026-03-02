<template>
  <div class="home">
    <h1>欢迎使用 Fantnel 管理系统</h1>

    <div class="intro">
      <p>Fantnel 是一个功能强大的游戏服务器管理系统，提供账号管理、服务器管理、插件管理等多种功能。</p>
    </div>

    <div class="features">

      <!-- 广告位 -->
      <div class="ads" v-if="adData && (adData.ad1 || adData.ad2 || adData.ad3)">
        <div class="ad-card" v-if="adData && adData.ad1">
          <h3>{{ adData.ad1.name || '广告位 1' }}</h3>
          <p>{{ adData.ad1.text || '这里可以放置广告内容，展示您的产品或服务。' }}</p>
        </div>
        <div class="ad-card" v-if="adData && adData.ad2">
          <h3>{{ adData.ad2.name || '广告位 2' }}</h3>
          <p>{{ adData.ad2.text || '这里可以放置广告内容，展示您的产品或服务。' }}</p>
        </div>
        <div class="ad-card" v-if="adData && adData.ad3">
          <h3>{{ adData.ad3.name || '广告位 3' }}</h3>
          <p>{{ adData.ad3.text || '这里可以放置广告内容，展示您的产品或服务。' }}</p>
        </div>
      </div>

      <!-- 拖拽区域 -->
      <div class="drag-drop-area" @dragover.prevent @drop.prevent="handleFileDrop">
        <div class="drag-drop-content">
          <p class="drag-drop-text">拖拽 主题 文件到此处</p>
          <p class="drag-drop-subtext">或点击选择文件</p>
          <a href="http://npyyds.top/fantnel/theme" target="_blank">
            <p class="drag-drop-subtext">下载主题，请前往 <b>涅槃科技</b> 下载</p>
          </a>
          <input type="file" accept=".fant.json" class="hidden" ref="fileInput" @change="handleFileSelect">
          <button class="drag-drop-button" @click="$refs.fileInput.click()">
            选择 主题 文件
          </button>
        </div>
      </div>

      <Alert :show="showModal" :message="modalMessage" title="提示" @ok="reload()" />

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getHome_Info, setThemeSwitch } from '../utils/Tools.js'

const showModal = ref(false)
const modalMessage = ref('')

// 广告数据
const adData = ref({
  ad1: { name: '', text: '' },
  ad2: { name: '', text: '' },
  ad3: { name: '', text: '' }
})

// 处理文件拖拽
const handleFileDrop = (event) => {
  const files = event.dataTransfer.files
  if (files.length > 0) {
    handleFile(files[0])
  }
}

// 处理文件选择
const handleFileSelect = (event) => {
  const files = event.target.files
  if (files.length > 0) {
    handleFile(files[0])
  }
}

// 处理文件
const handleFile = (file) => {
  if (file.name.endsWith('.fant.json')) {
    const reader = new FileReader()
    reader.onload = (e) => {
      try {
        showModal.value = true
        modalMessage.value = '正在应用主题，请稍后...'
        var json = JSON.parse(e.target.result)
        setThemeSwitch(json).then(data => {
          modalMessage.value = data.msg
        })
      } catch (error) {
        console.error('Fantnel 主题 解析错误:', error)
        alert('Fantnel 主题 文件解析失败，请检查文件格式。')
      }
    }
    reader.onerror = () => {
      alert('文件读取失败，请重试。')
    }
    reader.readAsText(file)
  } else {
    alert('请选择 Fantnel 主题 格式的文件。')
  }
}

const reload = () => {
  location.reload(true);
}

// 获取首页数据
onMounted(async () => {
  try {
    const data = await getHome_Info()
    adData.value = data
  } catch (error) {
    console.error('获取首页数据失败:', error)
  }
})
</script>

<style scoped>
.home {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

h1 {
  color: var(--text-color);
  margin-bottom: 20px;
  font-size: 2rem;
}

.intro {
  background-color: var(--sidebar-bg);
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 30px;
}

.intro p {
  color: var(--text-color);
  font-size: 1.1rem;
  line-height: 1.6;
}

.features {
  margin-top: 30px;
}

.features h2 {
  color: var(--text-color);
  margin-bottom: 20px;
  font-size: 1.5rem;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
}

.feature-item {
  background-color: var(--sidebar-bg);
  padding: 20px;
  border-radius: 8px;
  transition: transform 0.3s ease;
}

.feature-item:hover {
  transform: translateY(-5px);
}

.feature-item h3 {
  color: var(--text-color);
  margin-bottom: 10px;
  font-size: 1.2rem;
}

.feature-item p {
  color: var(--text-color);
  font-size: 0.95rem;
  line-height: 1.5;
}

/* 广告位样式 */
.ads {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 30px;
  opacity: 0.7;
}

.ad-card {
  background-color: var(--ad-bg);
  padding: 15px;
  border-radius: 6px;
  font-size: 0.9rem;
}

.ad-card h3 {
  color: var(--text-color);
  margin-bottom: 8px;
  font-size: 1rem;
}

.ad-card p {
  color: var(--text-color);
  line-height: 1.4;
}

/* 拖拽区域样式 */
.drag-drop-area {
  background-color: var(--sidebar-bg);
  border: 2px dashed var(--border-color);
  border-radius: 8px;
  padding: 32px;
  margin-bottom: 30px;
  transition: border-color 0.3s ease;
}

.drag-drop-area:hover {
  border-color: var(--primary-color);
}

.drag-drop-content {
  text-align: center;
}

.drag-drop-text {
  color: var(--text-color);
  margin-bottom: 8px;
  font-size: 1rem;
}

.drag-drop-subtext {
  color: var(--text-muted);
  font-size: 0.875rem;
  margin-bottom: 16px;
}

.drag-drop-subtext a {
  color: var(--primary-color);
  text-decoration: none;
}

.drag-drop-subtext a:hover {
  text-decoration: underline;
}

.hidden {
  display: none;
}

.drag-drop-button {
  margin-top: 16px;
  padding: 8px 16px;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
</style>