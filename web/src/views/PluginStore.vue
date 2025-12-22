<template>
  <div class="plugin-store">
    <h1>插件商城</h1>
    <p>插件商城页面，用于浏览和下载系统中的插件。</p>
    <div class="search-bar">
      <input type="text" v-model="searchQuery" placeholder="搜索插件...">
    </div>
    <div class="plugin-list">
      <div v-for="plugin in filteredPlugins" :key="plugin.id" class="plugin-item" @click="navigateToPlugin(plugin.id)">
        <div class="plugin-info">
          <h3>{{ plugin.name }}</h3>
          <p class="plugin-description">{{ plugin.shortDescription }}</p>
          <div class="plugin-meta">
            <span class="publisher">发布者: {{ plugin.publisher }}</span>
            <span class="download-count">下载次数: {{ plugin.downloadCount }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getPluginList } from '../utils/Tools'

const searchQuery = ref('')
const plugins = ref();
// [
//   {
//     id: "f110da9f-f0cb-f926-c72c-feac7fcf3601",
//     name: "Heypixel Protocol",
//     shortDescription: "A lightweight protocol plugin for Heypixel, enabling seamless compatibility and optimized performance.",
//     publisher: "DevCodexus",
//     downloadCount: 25275
//   }
// ]

// 获取插件列表
onMounted(async () => {
  plugins.value = await getPluginList().then(res => res.data);
});

const filteredPlugins = computed(() => {
  if (!searchQuery.value) return plugins.value
  return plugins.value.filter(plugin =>
    plugin.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    plugin.shortDescription.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    plugin.publisher.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

function navigateToPlugin(id) {
  location.href = `/plugin/${id}`
}

</script>

<style scoped>
.plugin-store {
  padding: 20px;
}

.search-bar {
  margin-bottom: 20px;
  margin-top: 10px;
}

.search-bar input {
  width: 100%;
  padding: 10px;
  border: 1px solid var(--border-color);
  border-radius: 5px;
  font-size: 16px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.plugin-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
  max-height: 600px;
  overflow-y: auto;
  padding: 10px;
}

.plugin-item {
  border: 1px solid var(--border-color);
  border-radius: 5px;
  overflow: hidden;
  cursor: pointer;
  transition: transform 0.2s;
  background-color: var(--bg-color);
}

.plugin-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.plugin-info {
  padding: 15px;
}

.plugin-info h3 {
  margin: 0 0 10px 0;
  font-size: 18px;
  color: var(--text-color);
}

.plugin-description {
  margin: 0 0 12px 0;
  color: var(--text-color);
  opacity: 0.8;
  font-size: 14px;
  line-height: 1.4;
}

.plugin-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
  color: var(--text-color);
  opacity: 0.6;
}

.publisher {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-right: 10px;
}

.download-count {
  font-weight: 500;
  color: var(--text-color);
  opacity: 0.8;
}
</style>