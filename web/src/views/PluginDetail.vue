<template>
  <div class="plugin-detail">
    <!-- Loading Dialog -->
    <div v-if="isLoading" class="loading-overlay">
      <div class="loading-dialog">
        <div class="loading-spinner"></div>
        <p v-if="!isLoadingError">{{ isLoadingText }}</p>
        <p v-else>
          {{ isLoadingText }}<br>
          <span class="countdown-text">将在 {{ countdown }} 秒后返回插件商店...</span>
        </p>
      </div>
    </div>

    <div class="plugin-header">
      <div class="plugin-title">
        <h1>{{ plugin.name }}</h1>
        <p>插件ID: {{ plugin.id }}</p>
      </div>
      <button class="download-btn" @click="showAlert = true">Download</button>
    </div>

    <Alert :show="showAlert" :title="'插件安装提醒'" :message="confirmMessage" :show-cancel="true" @ok="handleDownload"
      @cancel="showAlert = false" @close="showAlert = false" />
    <Alert :show="installAlertVisible" :title="'安装状态'" :message="installAlertMessage" :show-cancel="false" @ok="installAlertVisible = false" 
      @close="installAlertVisible = false" />

    <div class="plugin-images">
      <img v-if="plugin.logoUrl" :src="plugin.logoUrl" alt="插件Logo" class="main-image" width="256px" height="256px">
    </div>

    <div class="plugin-meta">
      <div class="meta-item">
        <span class="label">发布者:</span>
        <span class="value">{{ plugin.publisher }}</span>
      </div>
      <div class="meta-item">
        <span class="label">发布时间:</span>
        <span class="value">{{ plugin.publishDate }}</span>
      </div>
      <div class="meta-item">
        <span class="label">插件版本:</span>
        <span class="value">{{ plugin.version }}</span>
      </div>
      <div class="meta-item">
        <span class="label">下载数量:</span>
        <span class="value">{{ plugin.downloadCount }}</span>
      </div>
      <div class="meta-item">
        <span class="label">依赖项:</span>
        <div class="value">
          <template v-if="plugin.dependencies && plugin.dependencies.length > 0">
            <a v-for="dep in plugin.dependencies" :key="dep.id" :href="`/plugin/${dep.id}`" class="dependency-link">{{
              dep.name }}</a>
          </template>
          <template v-else>
            无依赖
          </template>
        </div>
      </div>
    </div>

    <div class="tab-content">
      <div class="introduction">
        <h2>插件介绍</h2>
        <div v-if="plugin.detailDescription" v-html="plugin.detailDescription"></div>
        <div v-else class="no-description">这个插件没有介绍自己呢。</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getPluginDetail, installPlugin } from '../utils/Tools'
import Alert from '../components/Alert.vue'

const route = useRoute();
const router = useRouter();
const pluginId = route.params.id;

const plugin = ref({});
const isLoading = ref(true);
const isLoadingText = ref('正在加载插件详情...');
const isLoadingError = ref(false);
const countdown = ref(4);
const showAlert = ref(false);

// {
//   id: pluginId,
//   name: `插件${pluginId}`,
//   publisher: '发布者名称',
//   publishDate: '2023-01-01',
//   version: '1.0.0',
//   downloadCount: 1234,
//   detailDescription: '这是一个详细的插件介绍，包含了插件的各种功能和使用方法。插件拥有丰富的配置选项和良好的兼容性，欢迎用户下载使用。' + ' '.repeat(100), // 填充到500字左右
//   logoUrl: '',
//   shortDescription: '',
//   dependencies: [
//         {
//           "id": "716925e5-feee-8199-5a7a-855d8e6bd85f",
//           "name": "Base1122"
//         }
//       ]
// }

onMounted(async () => {
  isLoading.value = true;
  getPluginDetail(pluginId).then(res => {
    if (res.code == 1) {
      plugin.value = res.data;
      isLoading.value = false;
    } else {
      isLoadingText.value = res.msg;
      isLoadingError.value = true;
    }
  }).catch((err) => {
    isLoadingText.value = err.message;
    isLoadingError.value = true;
  })
})

// 监听isLoadingError变化，实现错误时的倒计时跳转
watch(isLoadingError, (newValue) => {
  if (newValue) {
    countdown.value = 4;
    const timer = setInterval(() => {
      countdown.value--;
      if (countdown.value <= 0) {
        clearInterval(timer);
        router.push('/plugin-store');
      }
    }, 1000);
  }
})

// 弹窗消息相关
const confirmMessage = computed(() => {
  let dependenciesHtml = '';
  if (plugin.value.dependencies && plugin.value.dependencies.length > 0) {
    dependenciesHtml = plugin.value.dependencies.map(dep => `<a href="/plugin/${dep.id}" class="dependency-link" style="color: #2196F3; text-decoration: none; ">${dep.name}</a>`);
    return `<p>即将安装 ${plugin.value.name} 插件</p><p>并安装 ${dependenciesHtml} 依赖，确认进行安装吗？</p>`;
  } else {
    dependenciesHtml = '无';
    return `<p>即将安装 ${plugin.value.name} 插件</p><p>确认进行安装吗？</p>`;
  }
});

// 安装状态弹窗
const installAlertVisible = ref(false);
const installAlertMessage = ref("");

// 处理下载确认
const handleDownload = () => {
  showAlert.value = false;
  // 显示安装状态弹窗
  installAlertMessage.value = "正在安装插件，请稍后.....";
  installAlertVisible.value = true;
  installPlugin(plugin.value.id).then(data => {
    // 安装完成，更新消息
    installAlertMessage.value = data.msg;
  }).catch(err => {
    // 安装失败，更新消息
    installAlertMessage.value = err.message;
  });
}


</script>

<style scoped>
.plugin-detail {
  padding: 20px;
}

.plugin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.plugin-title h1 {
  margin: 0;
  font-size: 24px;
}

.plugin-title p {
  margin: 5px 0 0 0;
  color: var(--text-color);
}

.download-btn {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
}

.plugin-images {
  margin-bottom: 20px;
}

.main-image {
  height: auto;
  object-fit: cover;
  border-radius: 5px;
  margin-bottom: 10px;
}

.small-images {
  display: flex;
  gap: 10px;
}

.small-image {
  width: 80px;
  height: 48px;
  object-fit: cover;
  border-radius: 3px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: border-color 0.2s;
}

.small-image:hover {
  border-color: #2196F3;
}

.plugin-meta {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 20px;
  padding: 15px;
  background-color: var(--sidebar-bg);
  border-radius: 5px;
}

.meta-item {
  display: flex;
  flex-direction: column;
}

.meta-item .label {
  font-weight: bold;
  color: var(--text-color);
  font-size: 14px;
}

.meta-item .value {
  margin-top: 5px;
  font-size: 16px;
  color: var(--text-color);
}

.tab-content {
  background-color: var(--sidebar-bg);
  padding: 20px;
  border-radius: 5px;
}

.introduction h2 {
  margin-top: 0;
  color: var(--text-color);
}

.introduction p {
  line-height: 1.6;
  color: var(--text-color);
}

.dependency-link {
  color: #2196F3;
  text-decoration: none;
  margin-right: 10px;
}

.dependency-link:hover {
  text-decoration: underline;
}

.no-description {
  color: var(--text-color);
  font-style: italic;
  opacity: 0.7;
}

/* Loading Dialog Styles */
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
  background-color: white;
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
  border: 4px solid #f3f3f3;
  border-top: 4px solid #4CAF50;
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
  color: #333;
  font-weight: 500;
}
</style>