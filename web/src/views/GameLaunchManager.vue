<template>
  <div class="game-launch-manager">
    <h1>游戏启动管理</h1>
    <p>用于管理已启动的游戏实例</p>
    <div class="game-launch-header">
      <button class="close-all-btn" @click="showCloseAllConfirm" :disabled="!gameLaunchList.length">
        关闭全部游戏
      </button>
    </div>

    <div class="game-launch-stats" v-if="gameLaunchList.length">
      <p>当前运行的游戏实例数量: <strong>{{ gameLaunchList.length }}</strong></p>
    </div>

    <div class="game-launch-list" v-if="gameLaunchList.length">
      <table class="game-launch-table">
        <thead>
          <tr>
            <th>游戏名称</th>
            <th>角色名称</th>
            <th>游戏ID</th>
            <th>用户ID</th>
            <th>客户端类型</th>
            <th>游戏类型</th>
            <th>游戏版本</th>
            <th>服务器地址</th>
            <th>最大内存</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(game, index) in gameLaunchList" :key="game.game_id">
            <td>{{ game.game_name }}</td>
            <td>{{ game.role_name }}</td>
            <td>{{ game.game_id }}</td>
            <td>{{ game.user_id }}</td>
            <td>{{ game.client_type === 1 ? 'PC' : '移动端' }}</td>
            <td>{{ game.game_type === 2 ? 'MOD' : '原版' }}</td>
            <td>{{ game.game_version }}</td>
            <td>{{ game.server_ip }}:{{ game.server_port }}</td>
            <td>{{ game.max_game_memory }} MB</td>
            <td>
              <div class="action-buttons">
                <button class="close-btn" @click="showCloseSingleConfirm(index)">
                  关闭
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="empty-state" v-else>
      <p v-if="loading">正在加载游戏实例信息...</p>
      <p v-else>当前没有运行的游戏实例</p>
    </div>

    <!-- 确认对话框 -->
    <Alert :show="confirmVisible" :title="confirmTitle" :message="confirmMessage" :show-cancel="true" :ok-text="'确认'"
      :cancel-text="'取消'" @ok="handleConfirmOk" @cancel="handleConfirmCancel" @close="handleConfirmCancel" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getGameLaunchInfo, closeGameLaunch } from '../utils/Tools.js'
import Alert from '../components/Alert.vue'

// 游戏列表
const gameLaunchList = ref([])
// 加载状态
const loading = ref(false)

// 确认对话框状态
const confirmVisible = ref(false)
const confirmTitle = ref('')
const confirmMessage = ref('')
let confirmAction = null // 存储确认后的回调函数

// 获取游戏实例信息
const fetchGameLaunchInfo = async () => {
  loading.value = true
  try {
    const data = await getGameLaunchInfo()
    if (data.code === 1) {
      gameLaunchList.value = data.data
    } else {
      console.error('获取游戏实例信息失败:', data.msg)
    }
  } catch (error) {
    console.error('获取游戏实例信息失败:', error)
  } finally {
    loading.value = false
  }
}

// 显示关闭单个游戏实例确认
const showCloseSingleConfirm = (id) => {
  confirmTitle.value = '确认关闭游戏'
  confirmMessage.value = '确定要关闭此游戏实例吗？'
  confirmAction = () => handleCloseSingle(id)
  confirmVisible.value = true
}

// 显示关闭全部游戏实例确认
const showCloseAllConfirm = () => {
  confirmTitle.value = '确认关闭全部游戏'
  confirmMessage.value = `确定要关闭所有 ${gameLaunchList.value.length} 个游戏实例吗？`
  confirmAction = handleCloseAll
  confirmVisible.value = true
}

// 关闭单个游戏实例
const handleCloseSingle = async (index) => {
  try {
    const data = await closeGameLaunch(index)
    if (data.code === 1) {
      // 从列表中移除关闭的游戏实例
      gameLaunchList.value = gameLaunchList.value.filter((_, i) => i !== index)
    } else {
      console.error('关闭游戏实例失败:', data.msg)
    }
  } catch (error) {
    console.error('关闭游戏实例失败:', error)
  }
}

// 关闭全部游戏实例
const handleCloseAll = async () => {
  if (gameLaunchList.value.length === 0) return

  try {
    // 依次关闭所有游戏实例
    for (const game of gameLaunchList.value) {
      await closeGameLaunch(game.game_id)
    }
    // 清空游戏实例列表
    gameLaunchList.value = []
  } catch (error) {
    console.error('关闭全部游戏实例失败:', error)
  }
}

// 确认对话框 - 确认
const handleConfirmOk = () => {
  confirmVisible.value = false
  if (confirmAction) {
    confirmAction()
    confirmAction = null
  }
}

// 确认对话框 - 取消
const handleConfirmCancel = () => {
  confirmVisible.value = false
  confirmAction = null
}

// 页面加载时获取游戏实例信息
onMounted(async () => {
  await fetchGameLaunchInfo()
})
</script>

<style scoped>
.game-launch-manager {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

h1 {
  color: var(--text-color);
  font-size: 2rem;
}

.game-launch-header {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 20px;
}

.close-all-btn {
  background-color: var(--danger-color);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  transition: background-color 0.3s ease;
}

.close-all-btn:hover:not(:disabled) {
  background-color: var(--danger-hover);
}

.close-all-btn:disabled {
  background-color: var(--disabled-color);
  cursor: not-allowed;
}

.game-launch-stats {
  background-color: var(--sidebar-bg);
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.game-launch-stats p {
  color: var(--text-color);
  margin: 0;
  font-size: 1rem;
}

.game-launch-list {
  background-color: var(--sidebar-bg);
  border-radius: 8px;
  overflow: auto;
  max-height: calc(100vh - 300px);
  width: 100%;
  display: inline-block;
  position: relative;
}

.game-launch-table {
  width: 100%;
  border-collapse: collapse;
}

.game-launch-table th,
.game-launch-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
  color: var(--text-color);
}

.game-launch-table th {
  background-color: var(--header-bg);
  font-weight: 600;
  font-size: 0.95rem;
  white-space: nowrap;
}

.game-launch-table tr:last-child td {
  border-bottom: none;
}

.game-launch-table tr:hover {
  background-color: var(--hover-color);
}

.action-buttons {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.close-btn {
  background-color: #ff4d4f;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
  transition: background-color 0.3s ease;
}

.close-btn:hover {
  background-color: #ff7875;
}

.empty-state {
  background-color: var(--sidebar-bg);
  padding: 40px;
  border-radius: 8px;
  text-align: center;
}

.empty-state p {
  color: var(--text-color);
  font-size: 1.1rem;
  margin: 0;
}
</style>