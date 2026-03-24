<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">网络服务器</h1>
    
    <!-- 服务器列表 -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="server in networkServers" :key="server.entity_id" class="bg-gray-800 rounded-lg overflow-hidden hover:shadow-lg transition-shadow">
        <img :src="server.title_image_url" :alt="server.name" class="w-full h-40 object-cover">
        <div class="p-4">
          <h3 class="text-lg font-bold mb-2">{{ server.name }}</h3>
          <p class="text-gray-300 text-sm mb-4">{{ server.brief_summary }}</p>
          <div class="flex justify-between items-center">
            <span class="text-gray-400 text-sm">在线人数: {{ server.online_count }}</span>
            <router-link :to="`/network-server/${server.entity_id}`" class="text-blue-400 hover:underline">
              查看详情
            </router-link>
          </div>
        </div>
      </div>
    </div>
    
    <!-- 分页 -->
    <div class="mt-8 flex justify-center">
      <div class="flex gap-2">
        <button @click="loadServers(0, pageSize)" class="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600 transition-colors" :disabled="currentPage === 1">
          第一页
        </button>
        <button @click="loadServers((currentPage - 2) * pageSize, pageSize)" class="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600 transition-colors" :disabled="currentPage === 1">
          上一页
        </button>
        <span class="px-4 py-2 bg-gray-800 rounded">
          第 {{ currentPage }} 页
        </span>
        <button @click="loadServers(currentPage * pageSize, pageSize)" class="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600 transition-colors">
          下一页
        </button>
      </div>
    </div>
    
    <!-- 状态弹窗 -->
    <StatusModal
      :visible="showModal"
      :message="modalMessage"
      @close="handleModalClose"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getNetworkServers } from '../services/api'
import StatusModal from '../components/StatusModal.vue'

const router = useRouter()
const networkServers = ref([])
const currentPage = ref(1)
const pageSize = ref(10)
const showModal = ref(false)
const modalMessage = ref('')

// 初始化
onMounted(() => {
  loadServers(0, pageSize.value)
})

// 加载服务器列表
const loadServers = async (offset, size) => {
  try {
    const response = await getNetworkServers(offset, size)
    if (response.data.code === 1) {
      networkServers.value = response.data.data
      currentPage.value = offset / size + 1
    } else {
      modalMessage.value = response.data.msg || '服务器列表加载失败'
      showModal.value = true
    }
  } catch (error) {
    console.error('Failed to load network servers:', error)
    modalMessage.value = '服务器列表加载失败'
    showModal.value = true
  }
}

// 处理弹窗关闭
const handleModalClose = () => {
  showModal.value = false
  router.push('/game-account')
}
</script>
