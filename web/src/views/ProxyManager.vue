<template>
  <div class="proxy-manager">
    <h1>代理管理</h1>
    <p>用于管理已启动的代理服务器</p>
    <div class="proxy-header">
      <button class="close-all-btn" @click="showCloseAllConfirm" :disabled="!proxyList.length">
        关闭全部代理
      </button>
    </div>

    <div class="proxy-stats" v-if="proxyList.length">
      <p>当前运行的代理数量: <strong>{{ proxyList.length }}</strong></p>
    </div>

    <div class="proxy-list" v-if="proxyList.length">
      <table class="proxy-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>昵称</th>
            <th>本地地址</th>
            <th>转发地址</th>
            <th>服务器名称</th>
            <th>服务器版本</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="proxy in proxyList" :key="proxy.Id">
            <td>{{ proxy.Id }}</td>
            <td>{{ proxy.Interceptor.NickName }}</td>
            <td>{{ serverIp }}:{{ proxy.Interceptor.LocalPort }}</td>
            <td>{{ proxy.Interceptor.ForwardAddress }}:{{ proxy.Interceptor.ForwardPort }}</td>
            <td>{{ proxy.Interceptor.ServerName }}</td>
            <td>{{ proxy.Interceptor.ServerVersion }}</td>
            <td>
              <div class="action-buttons">
                <button class="copy-btn"
                  @click="copyLocalAddress(`${serverIp}:${proxy.Interceptor.LocalPort}`)"
                  title="复制本地地址">
                  复制
                </button>
                <button class="close-btn" @click="showCloseSingleConfirm(proxy.Id)">
                  关闭
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="empty-state" v-else>
      <p v-if="loading">正在加载代理信息...</p>
      <p v-else>当前没有运行的代理服务器</p>
    </div>

    <p v-if="showCopyTip" class="copy-success-message">已为您复制本地地址到剪贴板!</p>

    <!-- 确认对话框 -->
    <Alert :show="confirmVisible" :title="confirmTitle" :message="confirmMessage" :show-cancel="true" :ok-text="'确认'"
      :cancel-text="'取消'" @ok="handleConfirmOk" @cancel="handleConfirmCancel" @close="handleConfirmCancel" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getProxyServerInfo, closeProxyServer, isVersionSafe } from '../utils/Tools.js'
import Alert from '../components/Alert.vue'
import useClipboard from 'vue-clipboard3'

isVersionSafe(1);

// 复制功能
const { toClipboard } = useClipboard()
// 复制成功消息显示状态
const showCopyTip = ref(false)

// 复制本地地址
const copyLocalAddress = async (address) => {
  try {
    await toClipboard(address)
    // 显示复制成功提示
    showCopyTip.value = true
    // 2秒后自动隐藏
    setTimeout(() => {
      showCopyTip.value = false
    }, 2000)
  } catch (error) {
    console.error('复制失败:', error)
  }
}

// 代理列表
const proxyList = ref([])
// 加载状态
const loading = ref(false)
// 服务器IP地址
const serverIp = ref('')

// 确认对话框状态
const confirmVisible = ref(false)
const confirmTitle = ref('')
const confirmMessage = ref('')
let confirmAction = null // 存储确认后的回调函数

// 获取代理信息
const fetchProxyInfo = async () => {
  loading.value = true
  try {
    const data = await getProxyServerInfo()
    if (data.code === 1) {
      serverIp.value = data.data.ip
      proxyList.value = data.data.proxies
    } else {
      console.error('获取代理信息失败:', data.msg)
    }
  } catch (error) {
    console.error('获取代理信息失败:', error)
  } finally {
    loading.value = false
  }
}

// 显示关闭单个代理确认
const showCloseSingleConfirm = (id) => {
  confirmTitle.value = '确认关闭代理'
  confirmMessage.value = '确定要关闭此代理服务器吗？'
  confirmAction = () => handleCloseSingle(id)
  confirmVisible.value = true
}

// 显示关闭全部代理确认
const showCloseAllConfirm = () => {
  confirmTitle.value = '确认关闭全部代理'
  confirmMessage.value = `确定要关闭所有 ${proxyList.value.length} 个代理服务器吗？`
  confirmAction = handleCloseAll
  confirmVisible.value = true
}

// 关闭单个代理
const handleCloseSingle = async (id) => {
  try {
    const data = await closeProxyServer(id)
    if (data.code === 1) {
      // 从列表中移除关闭的代理
      proxyList.value = proxyList.value.filter(proxy => proxy.Id !== id)
    } else {
      console.error('关闭代理失败:', data.msg)
    }
  } catch (error) {
    console.error('关闭代理失败:', error)
  }
}

// 关闭全部代理
const handleCloseAll = async () => {
  if (proxyList.value.length === 0) return

  try {
    // 依次关闭所有代理
    for (const proxy of proxyList.value) {
      await closeProxyServer(proxy.Id)
    }
    // 清空代理列表
    proxyList.value = []
  } catch (error) {
    console.error('关闭全部代理失败:', error)
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

// 页面加载时获取代理信息
onMounted(async () => {
  await fetchProxyInfo()
})
</script>

<style scoped>
.proxy-manager {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

h1 {
  color: var(--text-color);
  font-size: 2rem;
}

.proxy-header {
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

.proxy-stats {
  background-color: var(--sidebar-bg);
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.proxy-stats p {
  color: var(--text-color);
  margin: 0;
  font-size: 1rem;
}

.proxy-list {
  background-color: var(--sidebar-bg);
  border-radius: 8px;
  overflow: auto;
  max-height: calc(100vh - 300px);
  width: 100%;
  display: inline-block;
  position: relative;
}

.proxy-table {
  width: 100%;
  border-collapse: collapse;
}

.proxy-table th,
.proxy-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
  color: var(--text-color);
}

.proxy-table th {
  background-color: var(--header-bg);
  font-weight: 600;
  font-size: 0.95rem;
}

.proxy-table tr:last-child td {
  border-bottom: none;
}

.proxy-table tr:hover {
  background-color: var(--hover-color);
}

.action-buttons {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.copy-btn {
  background-color: var(--sidebar-active);
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.85rem;
  transition: background-color 0.3s ease;
}

.copy-btn:hover {
  opacity: 0.9;
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