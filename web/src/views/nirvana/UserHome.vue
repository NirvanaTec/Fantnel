<template>
  <div class="user-page">
    <div class="user-header">
      <h1 class="user-title">用户中心</h1>
      <button class="logout-button" @click="handleLogout">退出登录</button>
    </div>

    <div class="user-container">
      <!-- 用户信息卡片 -->
      <div class="user-card">
        <h2 class="card-title">账号信息</h2>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="hideAccount" class="form-checkbox" id="hideAccount" type="checkbox">
              <Tooltip>
                <label class="form-checkbox-label" for="hideAccount">隐藏账号</label>
                <template #tooltip>不显示完整账号，刷新后生效</template>
              </Tooltip>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="friendlyMode" class="form-checkbox" id="friendlyMode" type="checkbox">
              <Tooltip width="255px">
                <label class="form-checkbox-label" for="friendlyMode">友好模式</label>
                <template #tooltip>其它 Fantnel 用户无法攻击您 【高级功能】</template>
              </Tooltip>
            </div>
          </div>
        </div>

        <div class="user-card-content">
          <p class="text-gray-400 mb-4">当前剩余 <span class="text-red-400">{{ info.days }}</span> 天。</p>
          <div class="aligned-row">
            <p>购买更多天数请前往
              <a href="http://npyyds.top/shop/" target="_blank">涅槃科技</a> 进行购买
            </p>
            <Tooltip width="230px">
              <p class="text-gray-400 mb-4">关于高级功能 </p>
              <template #tooltip>您需要购买天数，才能使用高级功能。</template>
            </Tooltip>
          </div>
        </div>

      </div>

    </div>

    <!-- 加载组件 -->
    <Loading :visible="loadingVisible" :text="loadingText" :error="true" :countdown="countdown" />
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { logoutNirvana, getNirvanaAccount, hideNirvanaAccount, friendlyNirvana } from '../../utils/Tools.js'
import { Message } from '../../utils/message.js'
import Tooltip from '../../components/Tooltip.vue'

const hideAccount = ref(false)
const friendlyMode = ref(false)
const info = ref({})
const loadingVisible = ref(false)
const loadingText = ref('获取账号信息失败')
const countdown = ref(1)
const isInitialLoading = ref(true)

// 监听倒计时变化，实现跳转
watch(() => countdown.value, (newValue) => {
  if (newValue === 0 && loadingVisible.value) {
    window.location.href = '/login'
  }
})

// 监听隐藏账号变化
watch(hideAccount, (newValue) => {
  if (!isInitialLoading.value) {
    handleHideAccount(newValue)
  }
})

// 监听友好模式变化
watch(friendlyMode, (newValue) => {
  if (!isInitialLoading.value) {
    handleFriendlyMode(newValue)
  }
})

const loadAccountInfo = async () => {
  try {
    const data = await getNirvanaAccount()
    if (data.code === 1) {
      info.value = data.data || {}
      hideAccount.value = data.data?.hideAccount || false
      friendlyMode.value = data.data?.friendly || false
    } else {
      goBackToLogin(data?.msg || '获取账号信息失败')
    }
  } catch (error) {
    Message.error('获取账号信息失败，请检查网络连接')
    console.log(error)
  } finally {
    // 加载完成后设置为 false，允许后续的保存操作
    setTimeout(() => {
      isInitialLoading.value = false
    }, 0)
  }
}

const handleHideAccount = async (value) => {
  try {
    const data = await hideNirvanaAccount(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleFriendlyMode = async (value) => {
  try {
    const data = await friendlyNirvana(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleLogout = async () => {
  try {
    const data = await logoutNirvana()
    if (data.code === 1) {
      Message.success(data.msg || '退出登录成功')
      // 可以在这里添加跳转到登录页的逻辑
    } else {
      Message.warning(data?.msg || '退出登录失败')
    }
    goBackToLogin(data?.msg || '退出登录失败', 0)
  } catch (error) {
    Message.error('退出登录失败，请检查网络连接')
  }
}

// 返回登录页
const goBackToLogin = (msg, time = 1) => {
  // 显示加载组件并开始倒计时
  loadingVisible.value = true
  loadingText.value = msg
  countdown.value = time

  // 倒计时逻辑
  const timer = setInterval(() => {
    if (countdown.value > 0) {
      countdown.value--
    } else {
      clearInterval(timer)
    }
  }, 1000)
}

onMounted(() => {
  loadAccountInfo()
})
</script>

<style scoped>
.user-page {
  min-height: 100vh;
  padding: 40px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.user-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 40px;
}

.user-title {
  font-size: 2rem;
  font-weight: bold;
  color: var(--text-color);
}

.logout-button {
  padding: 10px 20px;
  background-color: var(--sidebar-active);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.logout-button:hover {
  background-color: rgba(66, 133, 244, 0.9);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(66, 133, 244, 0.3);
}

.logout-button:active {
  transform: translateY(0);
}

.user-container {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 30px;
}

.user-card {
  background-color: var(--sidebar-bg);
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  padding: 30px;
  border: 1px solid var(--border-color);
}

.card-title {
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 24px;
  color: var(--text-color);
}

.form-group {
  margin-bottom: 30px;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.remember-me {
  display: flex;
  align-items: center;
}

.form-checkbox {
  width: 16px;
  height: 16px;
  margin-right: 8px;
  accent-color: var(--sidebar-active);
}

.form-checkbox-label {
  font-size: 0.9rem;
  color: var(--text-color);
  opacity: 0.8;
}

.user-card-content {
  display: flex;
  flex-direction: column;
  margin-top: 20px;
}

.aligned-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
}

.account-info {
  margin-top: 20px;
}

.info-item {
  font-size: 1rem;
  margin-bottom: 12px;
  color: var(--text-color);
}

.info-item span {
  font-weight: 500;
  margin-left: 8px;
}

/* Fantnel Project Card */
.card-hover {
  transition: all 0.3s ease;
}

.card-hover:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .user-page {
    padding: 20px;
  }

  .user-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 20px;
  }

  .user-container {
    grid-template-columns: 1fr;
  }

  .user-card {
    padding: 20px;
  }
}
</style>