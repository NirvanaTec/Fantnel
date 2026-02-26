<template>
  <div class="settings-page">
    <div class="settings-header">
      <h1 class="settings-title">设置</h1>
    </div>

    <div class="settings-container">
      <!-- 自动登录配置卡片 -->
      <div class="settings-card">
        <h2 class="card-title">自动登录</h2>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="isAutoLoginGame" class="form-checkbox" id="autoLoginGame" type="checkbox">
              <label class="form-checkbox-label" for="autoLoginGame">开启主动登录</label>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="autoLoginGameCookie" class="form-checkbox" id="autoLoginGameCookie" type="checkbox">
              <label class="form-checkbox-label" for="autoLoginGameCookie">开启主动登录Cookie</label>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="isAutoLoginGame163Email" class="form-checkbox" id="autoLoginGame163Email" type="checkbox">
              <label class="form-checkbox-label" for="autoLoginGame163Email">开启主动登录163Email</label>
            </div>
          </div>
        </div>

      </div>

      <!-- IRC 配置卡片 -->
      <div class="settings-card">
        <h2 class="card-title">Chat | IRC</h2>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="ircEnabled" class="form-checkbox" id="ircEnabled" type="checkbox">
              <label class="form-checkbox-label" for="ircEnabled">是否开启</label>
            </div>
          </div>
        </div>

        <div class="form-group">
          <div class="form-options" style="margin-bottom: 10px;">
            <div class="remember-me">
              <input v-model="playerIdentifier" class="form-checkbox" id="playerIdentifier" type="checkbox">
              <label class="form-checkbox-label" for="playerIdentifier">玩家标识</label>
            </div>
          </div>
          <div style="display: flex; align-items: center; gap: 10px;">
            <label class="form-checkbox-label" for="chatPrefix">标记前缀</label>
            <input v-model="chatPrefixValue" class="form-input" id="chatPrefix" type="text" :disabled="!ircEnabled"
              style="width: 150px;">
          </div>
        </div>

      </div>

      <!-- 启动配置卡片 -->
      <div class="settings-card">
        <h2 class="card-title">启动配置</h2>

        <div class="form-group">
          <div class="form-options">
            <div class="remember-me">
              <input v-model="useJavaW" class="form-checkbox" id="useJavaW" type="checkbox">
              <label class="form-checkbox-label" for="useJavaW">使用JavaW</label>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">游戏内存: {{ gameMemory }}MB ({{ (gameMemory / 1024).toFixed(1) }}G)</label>
          <input v-model="gameMemory" type="range" class="form-slider" min="1024" max="18432" step="512">
          <div class="slider-labels">
            <span>1G</span>
            <span>18G</span>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">虚拟机参数</label>
          <textarea v-model="vmArgs" class="form-textarea" placeholder="输入虚拟机参数"></textarea>
        </div>

        <div class="form-group">
          <label class="form-label">游戏参数</label>
          <textarea v-model="gameArguments" class="form-textarea" placeholder="输入游戏参数"></textarea>
        </div>
      </div>


    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import { Message } from '../utils/message.js'
import { chatEnable, chatTarget, chatPrefix, jvmArgs, gameArgs, gameMemory as setGameMemory, getSettings, autoLoginGame, autoLoginGame163Email, setUseJavaW, autoLoginGameCookie as setAutoLoginGameCookie } from '../utils/Tools.js'

const vmArgs = ref('')
const gameArguments = ref('')
const gameMemory = ref('')
const useJavaW = ref(false)
const ircEnabled = ref(false)
const playerIdentifier = ref(false)
const chatPrefixValue = ref('')
const isAutoLoginGame = ref(false)
const isAutoLoginGame163Email = ref(false)
const autoLoginGameCookie = ref(false)
const isInitialLoading = ref(true)

// 监听 IRC 开启状态变化
watch(ircEnabled, (newValue) => {
  if (!isInitialLoading.value) {
    handleChatEnable(newValue)
  }
})

// 监听玩家标识状态变化
watch(playerIdentifier, (newValue) => {
  if (!isInitialLoading.value) {
    handleChatTarget(newValue)
  }
})

// 监听虚拟机参数变化
watch(vmArgs, (newValue) => {
  if (!isInitialLoading.value) {
    handleJvmArgs(newValue)
  }
})

// 监听游戏参数变化
watch(gameArguments, (newValue) => {
  if (!isInitialLoading.value) {
    handleGameArgs(newValue)
  }
})

// 监听游戏内存变化
watch(gameMemory, (newValue) => {
  if (!isInitialLoading.value) {
    handleGameMemory(newValue)
  }
})

// 监听聊天标记前缀变化
watch(chatPrefixValue, (newValue) => {
  if (!isInitialLoading.value) {
    handleChatPrefix(newValue)
  }
})

// 监听自动登录状态变化
watch(isAutoLoginGame, (newValue) => {
  if (!isInitialLoading.value) {
    handleAutoLoginGame(newValue)
  }
})

// 监听自动登录163Email状态变化
watch(isAutoLoginGame163Email, (newValue) => {
  if (!isInitialLoading.value) {
    handleAutoLoginGame163Email(newValue)
  }
})

// 监听自动登录Cookie状态变化
watch(autoLoginGameCookie, (newValue) => {
  if (!isInitialLoading.value) {
    handleAutoLoginGameCookie(newValue)
  }
})

// 监听使用JavaW状态变化
watch(useJavaW, (newValue) => {
  if (!isInitialLoading.value) {
    handleUseJavaW(newValue)
  }
})

onMounted(() => {
  // 加载设置的逻辑
  loadSettings()
})

const loadSettings = async () => {
  try {
    const data = await getSettings()
    if (data.code === 1) {
      // 加载设置数据
      vmArgs.value = data.data.jvmArgs || ''
      gameArguments.value = data.data.gameArgs || ''
      gameMemory.value = data.data.gameMemory || '4096'
      useJavaW.value = data.data.useJavaW || false
      ircEnabled.value = data.data.chatEnable || false
      playerIdentifier.value = data.data.chatTarget || false
      chatPrefixValue.value = data.data.chatPrefix || ''
      isAutoLoginGame.value = data.data.autoLoginGame || false
      autoLoginGameCookie.value = data.data.autoLoginGameCookie || false
      isAutoLoginGame163Email.value = data.data.autoLoginGame163Email || false
    } else {
      Message.warning(data.msg || '加载设置失败')
    }
  } catch (error) {
    Message.error('加载设置失败，请检查网络连接')
  } finally {
    // 使用 nextTick 确保所有监听器都已经处理完初始加载的数据
    await nextTick()
    // 加载完成后设置为 false，允许后续的保存操作
    isInitialLoading.value = false
  }
}

const handleChatEnable = async (value) => {
  try {
    const data = await chatEnable(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleChatTarget = async (value) => {
  try {
    const data = await chatTarget(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleJvmArgs = async (value) => {
  try {
    const data = await jvmArgs(value)
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleGameArgs = async (value) => {
  try {
    const data = await gameArgs(value)
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleGameMemory = async (value) => {
  try {
    const data = await setGameMemory(value)
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleChatPrefix = async (value) => {
  try {
    const data = await chatPrefix(value)
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleAutoLoginGame = async (value) => {
  try {
    const data = await autoLoginGame(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

const handleAutoLoginGame163Email = async (value) => {
  try {
    const data = await autoLoginGame163Email(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

// 处理使用JavaW状态变化
const handleUseJavaW = async (value) => {
  try {
    const data = await setUseJavaW(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

// 处理自动登录Cookie状态变化
const handleAutoLoginGameCookie = async (value) => {
  try {
    const data = await setAutoLoginGameCookie(value ? "true" : "false")
    if (data.code === 1) {
      Message.success(data.msg || '设置成功')
    } else {
      Message.warning(data.msg || '设置失败')
    }
  } catch (error) {
    Message.error('设置失败，请检查网络连接')
  }
}

</script>

<style scoped>
.settings-page {
  min-height: 100vh;
  padding: 40px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.settings-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 40px;
}

.settings-title {
  font-size: 2rem;
  font-weight: bold;
  color: var(--text-color);
}

.back-button {
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

.back-button:hover {
  background-color: rgba(66, 133, 244, 0.9);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(66, 133, 244, 0.3);
}

.back-button:active {
  transform: translateY(0);
}

.settings-container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.settings-card {
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
  margin-bottom: 24px;
}

.form-label {
  display: block;
  font-size: 1rem;
  font-weight: 500;
  margin-bottom: 8px;
  color: var(--text-color);
}

.form-textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background-color: var(--bg-color);
  color: var(--text-color);
  font-size: 0.9rem;
  resize: vertical;
  min-height: 80px;
}

.form-input {
  width: 100%;
  padding: 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background-color: var(--bg-color);
  color: var(--text-color);
  font-size: 0.9rem;
}

.form-options {
  display: flex;
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

.form-slider {
  width: 100%;
  margin: 10px 0;
  accent-color: var(--sidebar-active);
  cursor: pointer;
}

.slider-labels {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
  color: var(--text-color);
  opacity: 0.6;
  margin-top: 5px;
}

.settings-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.save-button {
  padding: 12px 32px;
  background-color: var(--sidebar-active);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.save-button:hover {
  background-color: rgba(66, 133, 244, 0.9);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(66, 133, 244, 0.3);
}

.save-button:active {
  transform: translateY(0);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .settings-page {
    padding: 20px;
  }

  .settings-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 20px;
  }

  .settings-card {
    padding: 20px;
  }
}
</style>