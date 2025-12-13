<template>
  <div class="servers">
    <h1>服务器管理</h1>
    <div class="search-bar">
      <input type="text" v-model="searchQuery" placeholder="搜索服务器...">
    </div>
    <div class="server-list" @scroll="handleScroll">
      <div v-for="server in filteredServers" class="server-item" @click="navigateToServer(server.entity_id)">
        <div class="server-image">
          <img :src="server.title_image_url" alt="服务器介绍图" width="307" height="173">
        </div>
        <div class="server-info">
          <h3>{{ server.name }}</h3>
          <p>{{ server.brief_summary }}</p>
        </div>
      </div>
    </div>

    <!-- 使用Alert组件 -->
    <Alert 
      :show="showNotice" 
      :message="noticeText" 
      title="注意事项"
      :location="noticeLocation"
      @ok="handleNoticeOk"
      @close="showNotice = false"
    />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
// import { useRouter } from 'vue-router'
import { getServerList } from '../utils/Tools'
// const router = useRouter()
import Alert from '../components/Alert.vue'

const searchQuery = ref('')
const servers = ref()

const offset = ref(0)

// 提示框
const showNotice = ref(false)
const noticeText = ref("")
const noticeLocation = ref("")

// [
//   { entity_id: 1, name: '服务器1', brief_summary: '这是一个很棒的服务器', title_image_url: 'https://via.placeholder.com/307x173' },
//   { entity_id: 2, name: '服务器2', brief_summary: '这是另一个很棒的服务器', title_image_url: 'https://via.placeholder.com/307x173' },
//   { entity_id: 3, name: '服务器3', brief_summary: '这是第三个很棒的服务器', title_image_url: 'https://via.placeholder.com/307x173' }
// ]

getServerList().then(res => {
  if(res.code !== 1){
    noticeText.value = res.msg;
    showNotice.value = true;
    noticeLocation.value = "/game-accounts";
    return;
  }
  servers.value = res.data
})

new Promise(() => {
  let isLoading = setInterval(() => {
    loadMoreServers().then(ok => {
      if(ok){
        clearInterval(isLoading);
      }
    })
  }, 500)
})

const filteredServers = computed(() => {
  if (!searchQuery.value) return servers.value;
  return servers.value.filter(server =>
    server.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    server.brief_summary.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

function navigateToServer(id) {
  location.href = `/server/${id}`;
  // router.push(`/server/${id}`)
}

function handleScroll(event) {
  // const element = event.target;
  // if (element.scrollTop + element.clientHeight >= element.scrollHeight - 10) {
  //   // 滚动到底部，加载更多服务器
  //   loadMoreServers();
  // }
}

function loadMoreServers(pageSize = 10) {
  // const newServers = [
  //   { entity_id: servers.value.length + 1, name: `服务器${servers.value.length + 1}`, describrief_summarytion: `这是第${servers.value.length + 1}个服务器`, title_image_url: 'https://via.placeholder.com/307x173' },
  //   { entity_id: servers.value.length + 2, name: `服务器${servers.value.length + 2}`, brief_summary: `这是第${servers.value.length + 2}个服务器`, title_image_url: 'https://via.placeholder.com/307x173' }
  // ]
  // servers.value = [...servers.value, ...newServers]
  offset.value += 10
  return getServerList(offset.value, pageSize).then(res => {
    if (res.data.length === 0) {
      return true;
    }
    servers.value = [...servers.value, ...res.data]
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
.servers {
  padding: 20px;
}

.search-bar {
  margin-bottom: 20px;
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
  max-height: 600px;
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
  margin: 0;
  color: #666;
  font-size: 14px;
}


</style>