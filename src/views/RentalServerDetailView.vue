<template>
  <div class="p-6">
    <button @click="router.back()" class="mb-6 text-blue-400 hover:underline">
      ← 返回列表
    </button>

    <div v-if="serverDetail" class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold mb-2">{{ serverDetail.name }}</h1>
      </div>

      <!-- 服务器图片 -->
      <div>
        <img :src="serverDetail.image_url" :alt="serverDetail.name" class="rounded-lg w-full h-64 object-cover">
      </div>

      <!-- 服务器信息 -->
      <div class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">服务器信息</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="text-gray-400">游戏版本:</span>
            <span class="ml-2">{{ serverDetail.mc_version }}</span>
          </div>
          <div>
            <span class="text-gray-400">服务器地址:</span>
            <span class="ml-2">{{ serverDetail.address }}</span>
          </div>
          <div>
            <span class="text-gray-400">在线人数:</span>
            <span class="ml-2">{{ serverDetail.player_count }}/{{ serverDetail.capacity }}</span>
          </div>
          <div>
            <span class="text-gray-400">服务器类型:</span>
            <span class="ml-2">{{ serverDetail.server_type }}</span>
          </div>
        </div>
        <div class="mt-4">
          <h3 class="font-medium mb-2">服务器描述</h3>
          <p class="text-gray-300">{{ serverDetail.brief_summary }}</p>
        </div>
      </div>

      <!-- 角色信息 -->
      <div v-if="launchInfo" class="bg-gray-800 rounded-lg p-6">
        <h2 class="text-xl font-bold mb-4">角色管理</h2>

        <!-- 选择账号 -->
        <div class="mb-6">
          <h3 class="font-medium mb-2">选择游戏账号</h3>
          <div class="flex flex-wrap gap-2">
            <button v-for="account in launchInfo.accounts" :key="account.id" @click="switchAccount(account)"
              class="px-4 py-2 rounded transition-colors"
              :class="selectedAccount?.id === account.id ? 'bg-blue-500' : 'bg-gray-700 hover:bg-gray-600'">
              {{ account.name }}
            </button>
          </div>
        </div>

        <!-- 角色列表 -->
        <div class="mb-6">
          <div class="flex justify-between items-center mb-4">
            <h3 class="font-medium">角色列表</h3>
            <button @click="showAddRoleModal = true" class="text-blue-400 hover:underline">
              添加角色
            </button>
          </div>
          <div class="space-y-2">
            <div v-for="role in launchInfo.games" :key="role.entity_id"
              class="flex justify-between items-center p-3 bg-gray-700 rounded">
              <span>{{ role.name }}</span>
              <div class="flex gap-2">
                <button @click="launchProxy(role.name)" class="text-blue-400 hover:underline">
                  启动代理
                </button>
                <button @click="launchGame(role.name)" class="text-green-400 hover:underline">
                  启动游戏
                </button>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- 添加角色模态框 -->
    <div v-if="showAddRoleModal" class="fixed inset-0 bg-gray-900 bg-opacity-70 flex items-center justify-center z-50">
      <div class="bg-gray-800 rounded-lg p-6 w-96">
        <h2 class="text-xl font-bold mb-4">添加角色</h2>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-gray-400 mb-1">角色名称</label>
            <input type="text" v-model="newRoleName"
              class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div class="flex gap-2">
            <button @click="addRole" class="flex-1 bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
              添加
            </button>
            <button @click="showAddRoleModal = false"
              class="flex-1 bg-gray-700 hover:bg-gray-600 rounded px-4 py-2 transition-colors">
              取消
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 启动代理提示框 -->
    <StatusModal :visible="showProxyModal" title="启动代理" :message="proxyMessage" @close="showProxyModal = false" />

    <!-- 启动游戏提示框 -->
    <StatusModal :visible="showGameModal" title="启动游戏" :message="gameMessage" @close="showGameModal = false" />

    <!-- 状态弹窗 -->
    <StatusModal :visible="showModal" :title="'提示'" :message="modalMessage" @close="handleModalClose" />

    <!-- 错误提示框 -->
    <StatusModal :visible="showErrorModal" :title="'错误'" :message="errorMessage" @close="showErrorModal = false" />

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import StatusModal from '../components/StatusModal.vue'
import { getRentalServerDetail, getRentalServerLaunchInfo, launchProxy as apiLaunchProxy, launchGame as apiLaunchGame, createRentalServerRole, switchGameAccount as apiSwitchGameAccount,getCurrentGameAccount } from '../services/api'
import { generateRandomGameName } from '../tools'

const route = useRoute()
const router = useRouter()
const serverId = route.params.id

const serverDetail = ref(null)
const launchInfo = ref(null)
const selectedAccount = ref(null)
const showAddRoleModal = ref(false)
const showProxyModal = ref(false)
const showGameModal = ref(false)
const showModal = ref(false)
const showErrorModal = ref(false)
const newRoleName = ref('')
const proxyMessage = ref('代理正在启动中，请稍候...')
const gameMessage = ref('游戏正在启动中，请稍候...')
const modalMessage = ref('')
const errorMessage = ref('')

// 初始化
onMounted(() => {
  loadServerDetail()
  loadLaunchInfo()
  newRoleName.value = generateRandomGameName()
})

// 加载服务器详细信息
const loadServerDetail = async () => {
  try {
    const response = await getRentalServerDetail(serverId)
    if (response.data.code === 1) {
      serverDetail.value = response.data.data
    } else {
      modalMessage.value = response.data.msg || '服务器详情加载失败'
      showModal.value = true
    }
  } catch (error) {
    console.error('Failed to load server detail:', error)
    modalMessage.value = '服务器详情加载失败'
    showModal.value = true
  }
}

// 加载启动信息
const loadLaunchInfo = async () => {
  try {
    const response = await getRentalServerLaunchInfo(serverId)
    if (response.data.code === 1) {
      launchInfo.value = response.data.data
      if (launchInfo.value.accounts.length > 0) {
        await setCurrentAccount()
      }
    } else {
      modalMessage.value = response.data.msg || '启动信息加载失败'
      showModal.value = true
    }
  } catch (error) {
    console.error('Failed to load launch info:', error)
    modalMessage.value = '启动信息加载失败'
    showModal.value = true
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
      loadLaunchInfo()
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

// 启动代理
const launchProxy = async (roleName) => {
  proxyMessage.value = '代理正在启动中，请稍候...'
  showProxyModal.value = true
  try {
    const response = await apiLaunchProxy(serverId, roleName, 'rental')
    if (response.data.msg) {
      proxyMessage.value = response.data.msg
    }
  } catch (error) {
    console.error('Failed to launch proxy:', error)
    proxyMessage.value = '启动代理失败，请重试'
  }
}

// 启动游戏
const launchGame = async (roleName) => {
  gameMessage.value = '游戏正在启动中，请稍候...'
  showGameModal.value = true
  try {
    const response = await apiLaunchGame(serverId, roleName, 'rental')
    if (response.data.msg) {
      gameMessage.value = response.data.msg
    }
  } catch (error) {
    console.error('Failed to launch game:', error)
    gameMessage.value = '启动游戏失败，请重试'
  }
}

// 添加角色
const addRole = async () => {
  try {
    const response = await createRentalServerRole({ id: serverId, name: newRoleName.value })
    if (response.data.code === 1) {
      await loadLaunchInfo()
      showAddRoleModal.value = false
      newRoleName.value = generateRandomGameName()
    } else {
      errorMessage.value = response.data.msg || '添加角色失败'
      showErrorModal.value = true
    }
  } catch (error) {
    console.error('Failed to add role:', error)
    errorMessage.value = '网络错误，请稍后重试'
    showErrorModal.value = true
  }
}

// 处理弹窗关闭
const handleModalClose = () => {
  showModal.value = false
  router.push('/game-account')
}
</script>
