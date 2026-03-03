<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">我的皮肤</h1>

    <!-- 搜索栏 -->
    <div class="mb-6">
      <div class="flex gap-2">
        <input type="text" v-model="searchKeyword" placeholder="搜索皮肤"
          class="flex-1 bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
        <button @click="searchSkins" class="bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
          搜索
        </button>
      </div>
    </div>

    <!-- 皮肤列表 -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <div v-for="skin in skins" :key="skin.entity_id"
        class="bg-gray-800 rounded-lg overflow-hidden hover:shadow-lg transition-shadow">
        <img :src="skin.title_image_url" :alt="skin.name" class="w-full h-48 object-cover">
        <div class="p-4">
          <h3 class="text-lg font-bold mb-2">{{ skin.name }}</h3>
          <p class="text-gray-300 text-sm mb-2">{{ skin.brief_summary }}</p>
          <div class="grid grid-cols-2 gap-2 mb-4">
            <div>
              <span class="text-gray-400 text-sm">作者:</span>
              <span class="block">{{ skin.developer_name }}</span>
            </div>
            <div>
              <span class="text-gray-400 text-sm">下载量:</span>
              <span class="block">{{ skin.download_num }}</span>
            </div>
          </div>
          <router-link :to="`/my-skin/${skin.entity_id}`"
            class="block w-full bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors text-center">
            查看详情
          </router-link>
        </div>
      </div>
    </div>

    <!-- 分页 -->
    <div class="mt-8 flex justify-center">
      <div class="flex gap-2">
        <button @click="loadSkins(0, pageSize)"
          class="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600 transition-colors" :disabled="currentPage === 1">
          第一页
        </button>
        <button @click="loadSkins((currentPage - 2) * pageSize, pageSize)"
          class="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600 transition-colors" :disabled="currentPage === 1">
          上一页
        </button>
        <span class="px-4 py-2 bg-gray-800 rounded">
          第 {{ currentPage }} 页
        </span>
        <button @click="loadSkins(currentPage * pageSize, pageSize)"
          class="px-4 py-2 bg-gray-700 rounded hover:bg-gray-600 transition-colors">
          下一页
        </button>
      </div>
    </div>

    <!-- 状态弹窗 -->
    <StatusModal :visible="showModal" :title="'提示'" :message="modalMessage" @close="handleModalClose" />

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getGameSkins } from '../services/api'
import StatusModal from '../components/StatusModal.vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const skins = ref([])
const currentPage = ref(1)
const pageSize = ref(10)
const searchKeyword = ref('')
const showModal = ref(false)
const modalMessage = ref('')

// 初始化
onMounted(() => {
  loadSkins(0, pageSize.value)
})

// 加载皮肤列表
const loadSkins = async (offset, size, keyword = '') => {
  try {
    const response = await getGameSkins(offset, size, keyword)
    if (response.data.code === 1) {
      skins.value = response.data.data
      currentPage.value = offset / size + 1
    } else {
      modalMessage.value = response.data.msg || '皮肤列表加载失败'
      showModal.value = true
    }
  } catch (error) {
    console.error('Failed to load skins:', error)
    modalMessage.value = '皮肤列表加载失败'
    showModal.value = true
  }
}

// 搜索皮肤
const searchSkins = () => {
  loadSkins(0, pageSize.value, searchKeyword.value)
}

// 处理弹窗关闭
const handleModalClose = () => {
  showModal.value = false
  router.push('/game-account')
}

</script>
