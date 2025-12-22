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
      <h2>主要功能</h2>
      <div class="feature-grid">
        <div class="feature-item">
          <h3>游戏账号管理</h3>
          <p>轻松管理您的游戏账号，支持添加、编辑、删除账号信息。</p>
        </div>
        <div class="feature-item">
          <h3>服务器管理</h3>
          <p>管理您的游戏服务器，查看服务器信息，一键启动服务器。</p>
        </div>
        <div class="feature-item">
          <h3>插件管理</h3>
          <p>管理服务器插件，查看插件状态，支持启动和停止插件。</p>
        </div>
        <div class="feature-item">
          <h3>插件商城</h3>
          <p>浏览和下载各种插件，丰富您的服务器功能。</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getHome_Info } from '../utils/Tools.js'

// 广告数据
const adData = ref({
  ad1: { name: '', text: '' },
  ad2: { name: '', text: '' },
  ad3: { name: '', text: '' }
})

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
</style>