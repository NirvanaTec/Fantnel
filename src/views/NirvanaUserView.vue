<template>
  <div class="p-6">
    <button @click="router.back()" class="mb-6 text-blue-400 hover:underline">
      ← 返回
    </button>

    <div v-if="nirvanaAccount" class="space-y-6">
      <h1 class="text-2xl font-bold">涅槃用户页</h1>

      <!-- 账号信息 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">账号信息</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="text-gray-400">账号:</span>
            <span class="ml-2">{{ nirvanaAccount.account }}</span>
          </div>
          <div>
            <span class="text-gray-400">会员天数:</span>
            <span class="ml-2">{{ nirvanaAccount.days }}</span>
          </div>
          <div>
            <span class="text-gray-400">隐藏账号:</span>
            <span class="ml-2">{{ nirvanaAccount.hideAccount ? '是' : '否' }}</span>
          </div>

        </div>
      </div>

      <!-- 设置选项 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">设置选项</h2>
        <div class="space-y-4">
          <div class="flex justify-between items-center">
            <span>设置是否隐藏账号</span>
            <button @click="toggleHideAccount"
              class="bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
              {{ nirvanaAccount.hideAccount ? '显示账号' : '隐藏账号' }}
            </button>
          </div>

        </div>
      </div>

      <!-- 购买链接 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <p>购买更多天数请前往<a href="http://npyyds.top/shop/" target="_blank" class="text-blue-400 hover:underline">涅槃科技</a>
          进行购买</p>
      </div>

      <!-- 操作 -->
      <div class="flex gap-2">
        <button @click="logout" class="bg-red-500 hover:bg-red-600 rounded px-4 py-2 transition-colors">
          登出涅槃账号
        </button>
      </div>
    </div>

    <div v-else class="bg-gray-800 rounded-lg p-6 text-center">
      <p class="text-gray-400">未登录涅槃账号</p>
      <button @click="router.push('/')" class="mt-4 text-blue-400 hover:underline">
        去登录
      </button>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getNirvanaAccountInfo, logoutNirvanaAccount, setSystemConfig } from '../services/api'

const router = useRouter()
const nirvanaAccount = ref(null)

// 初始化
onMounted(() => {
  loadAccountInfo()
})

// 加载账号信息
const loadAccountInfo = async () => {
  try {
    const response = await getNirvanaAccountInfo()
    if (response.data.code === 1) {
      nirvanaAccount.value = response.data.data
    } else {
      console.log("未登录涅槃账号\n" + response.data.msg)
      router.push('/')
    }
  } catch (error) {
    console.error('Failed to load nirvana account:', error)
  }
}

// 登出
const logout = async () => {
  try {
    const response = await logoutNirvanaAccount()
    if (response.data.code === 1) {
      nirvanaAccount.value = null
      location.reload()
    }
  } catch (error) {
    console.error('Logout failed:', error)
  }
}

// 切换隐藏账号状态
const toggleHideAccount = async () => {
  try {
    const newState = !nirvanaAccount.value.hideAccount
    const response = await setSystemConfig('hideAccount', newState)
    if (response.data.code === 1) {
      nirvanaAccount.value.hideAccount = newState
      location.reload()
    }
  } catch (error) {
    console.error('Failed to toggle hide account:', error)
  }
}

</script>
