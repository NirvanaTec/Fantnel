<template>
  <div class="game-rental">
    <h1>租赁服管理</h1>
    <p>租赁服页面，用于启动租赁服上的游戏。</p>
    <div class="search-bar">
      <input type="text" v-model="searchQuery" placeholder="搜索租赁服...">
    </div>
    <div class="server-list" @scroll="handleScroll">
      <div v-for="server in filteredServers" class="server-item" @click="navigateToServer(server.entity_id)">
        <div class="server-image">
          <img :src="server.image_url" alt="服务器介绍图" width="307" height="173">
        </div>
        <div class="server-info">
          <h3>{{ server.server_name }}</h3>
          <p>{{ server.brief_summary }}</p>
          <div class="server-meta">
            <span class="meta-item">版本: {{ server.mc_version }}</span>
            <span class="meta-item">在线: {{ server.player_count }}/{{ server.capacity }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 使用Alert组件 -->
    <Alert :show="showNotice" :message="noticeText" title="注意事项" :location="noticeLocation" @ok="handleNoticeOk"
      @close="showNotice = false" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { getRentalServerList, sortRentalServer } from '../utils/Tools'
import Alert from '../components/Alert.vue'

const searchQuery = ref('')
const servers = ref([])

const offset = ref(0)

// 提示框
const showNotice = ref(false)
const noticeText = ref("")
const noticeLocation = ref("")

const isActive = ref(true)

onMounted(async () => {
  isActive.value = true
  // 启动批量加载
  await sortRentalServer();
  await loadServersInBatches()
})

onUnmounted(() => {
  isActive.value = false
})

// 初始加载服务器列表
async function loadServersInBatches() {
  // 只有第一次加载错误时，才抛出异常
  let loading = true;
  while (true) {

    // 是否在当前页
    if (!isActive.value) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      continue;
    }

    const ok = await loadMoreServers(15, loading)
    loading = false;
    // 如果没有更多服务器可加载，则跳出循环
    if (!ok) {
      break
    }
    // 每次加载完服务器后，等待 600 毫秒
    await new Promise(resolve => setTimeout(resolve, 700))
  }
}

const filteredServers = computed(() => {
  if (!searchQuery.value) return servers.value;
  return servers.value.filter(server =>
    server.server_name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    server.brief_summary.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

function navigateToServer(id) {
  location.href = `/game-rental/${id}`;
}

function handleScroll(event) {
  // const element = event.target;
  // if (element.scrollTop + element.clientHeight >= element.scrollHeight - 10) {
  //   // 滚动到底部，加载更多服务器
  //   loadMoreServers();
  // }
}

function loadMoreServers(pageSize = 10, throwError = true) {
  return getRentalServerList(offset.value, pageSize).then(res => {
    if (res.code !== 1) {
      if (throwError) {
        noticeText.value = res.msg || "获取租赁服列表失败"
        showNotice.value = true
        noticeLocation.value = "/game-accounts";
      }
      return false;
    }
    if (!res || !res.data || !res.data.length) {
      return false;
    }
    servers.value = [...servers.value, ...res.data]
    offset.value += pageSize
    return true;
  }).catch(err => {
    noticeText.value = err.message || "获取租赁服列表异常"
    showNotice.value = true
    noticeLocation.value = "/game-accounts";
    return false;
  })
}

function handleNoticeOk() {
  showNotice.value = false;
  if (noticeLocation.value && noticeLocation.value !== '') {
    location.href = noticeLocation.value;
  }
}
</script>

<style scoped>
.game-rental {
  padding: 20px;
}

.search-bar {
  margin-bottom: 20px;
  margin-top: 10px;
}

.search-bar input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 16px;
}

.server-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
  max-height: 80vh;
  overflow-y: auto;
  padding: 10px;
}

.server-item {
  border: 1px solid #ddd;
  border-radius: 5px;
  overflow: hidden;
  cursor: pointer;
  transition: transform 0.2s;
}

.server-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.server-image img {
  width: 100%;
  height: auto;
  object-fit: cover;
}

.server-info {
  padding: 15px;
}

.server-info h3 {
  margin: 0 0 10px 0;
  font-size: 18px;
}

.server-info p {
  margin: 0 0 10px 0;
  color: #666;
  font-size: 14px;
}

.server-meta {
  display: flex;
  gap: 15px;
  font-size: 12px;
  color: #999;
}

.meta-item {
  background-color: #f5f5f5;
  padding: 3px 8px;
  border-radius: 10px;
}
</style>