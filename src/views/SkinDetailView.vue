<template>
  <div class="p-6">
    <button @click="router.back()" class="mb-6 text-blue-400 hover:underline">
      ← 返回列表
    </button>
    
    <div v-if="skinDetail" class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold mb-2">{{ skinDetail.name }}</h1>
        <p class="text-gray-400">作者: {{ skinDetail.developer_name }} | 发布时间: {{ formatDate(skinDetail.publish_time) }}</p>
      </div>
      
      <!-- 皮肤图片 -->
      <div>
        <img :src="skinDetail.title_image_url" :alt="skinDetail.name" class="rounded-lg w-full h-64 object-cover">
      </div>
      
      <!-- 皮肤信息 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">皮肤信息</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="text-gray-400">作者:</span>
            <span class="ml-2">{{ skinDetail.developer_name }}</span>
          </div>
          <div>
            <span class="text-gray-400">下载量:</span>
            <span class="ml-2">{{ skinDetail.download_num }}</span>
          </div>
          <div>
            <span class="text-gray-400">点赞数:</span>
            <span class="ml-2">{{ skinDetail.like_num }}</span>
          </div>
        </div>
        <div class="mt-4">
          <h3 class="font-medium mb-2">皮肤描述</h3>
          <p class="text-gray-300">{{ skinDetail.brief_summary }}</p>
        </div>
      </div>
      
      <!-- 登录成功的游戏信息 -->
      <div v-if="availableAccounts.length > 0" class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">选择账号</h2>
        <div class="flex flex-wrap gap-2 mb-6">
          <button v-for="account in availableAccounts" :key="account.id" @click="switchAccount(account)" class="px-4 py-2 rounded transition-colors" :class="selectedAccount?.id === account.id ? 'bg-blue-500' : 'bg-gray-700 hover:bg-gray-600'">
            {{ account.name }}
          </button>
        </div>
      </div>
      
      <!-- 设置皮肤按钮 -->
      <div class="flex justify-center">
        <button @click="setSkin(skinDetail.entity_id)" class="bg-blue-500 hover:bg-blue-600 rounded px-8 py-3 transition-colors text-lg font-medium">
          设置为当前账号皮肤
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getSkinDetail, setSkin as apiSetSkin, getAvailableGameAccounts, getCurrentGameAccount, switchGameAccount as apiSwitchGameAccount } from '../services/api'

const route = useRoute()
const router = useRouter()
const skinId = route.params.id

const skinDetail = ref(null)
const availableAccounts = ref([])
const selectedAccount = ref(null)

// 初始化
onMounted(() => {
  loadSkinDetail()
  loadAvailableAccounts()
})

// 加载皮肤详情
const loadSkinDetail = async () => {
  try {
    const response = await getSkinDetail(skinId)
    if (response.data.code === 1) {
      skinDetail.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load skin detail:', error)
  }
}

// 加载可用游戏账号
const loadAvailableAccounts = async () => {
  try {
    const response = await getAvailableGameAccounts()
    if (response.data.code === 1) {
      availableAccounts.value = response.data.data
      if (availableAccounts.value.length > 0) {
         await setCurrentAccount()
      }
    }
  } catch (error) {
    console.error('Failed to load available accounts:', error)
  }
}

// 设置当前游戏账号
const setCurrentAccount = async () => {
  try {
    const currentAccountResponse = await getCurrentGameAccount()
    if (currentAccountResponse.data.code === 1) {
      selectedAccount.value = currentAccountResponse.data.data;
    }
  } catch (error) {
    console.error('Failed to get current game account:', error)
  }
}

// 切换账号
const switchAccount = async (account) => {
  try {
    const response = await apiSwitchGameAccount(account.id)
    if (response.data.code === 1) {
      loadAvailableAccounts()
    } else {
      errorMessage.value = response.data.msg || '账号切换失败'
      showErrorModal.value = true
    }
  } catch (error) {
    console.error('Failed to select account:', error)
    errorMessage.value = '网络错误，请稍后重试'
    showErrorModal.value = true
  }
}

// 设置皮肤
const setSkin = async (skinId) => {
  try {
    const response = await apiSetSkin(skinId)
    if (response.data.code === 1) {
      alert('皮肤设置成功')
    }
  } catch (error) {
    console.error('Failed to set skin:', error)
  }
}

// 格式化日期
const formatDate = (timestamp) => {
  const date = new Date(timestamp * 1000)
  return date.toLocaleDateString('zh-CN')
}
</script>
