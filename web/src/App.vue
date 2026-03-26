<script setup>
import { ref, provide, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getThemeName, setThemeName, getNirvanaAccount, initWindowMode, minimizeWindow, closeWindow, sendMessage } from './utils/Tools'

const route = useRoute()
const router = useRouter()
const theme = ref('dark')
const account = ref('')
const showCloseIcon = ref(false)
const showMinimizeIcon = ref(false)
const showWindowControls = ref(false)

// 计算是否显示导航栏
const showNavbar = computed(() => {
  return route.path !== '/login-socket'
})

// Initialize theme from API
onMounted(async () => {
  initDragZone();


  const themeName = await getThemeName().then(res => res.data);
  theme.value = themeName === 'default' ? 'dark' : themeName
  // 获取涅槃账号信息
  const accountInfo = await getNirvanaAccount().then(res => res.data);
  account.value = accountInfo.account
  // 初始化消息接收
  initMessageReceiver()
  // 初始化窗口模式
  await initWindowMode();


})

function toggleTheme() {
  // Calculate next theme
  const nextTheme = theme.value === 'light' ? 'dark' : theme.value === 'dark' ? 'gray' : 'light'
  // Set theme using API
  setThemeName(nextTheme)
  // Update local state
  theme.value = nextTheme
}

function handleMinimize() {
  minimizeWindow();
}

function handleClose() {
  closeWindow();
}

provide('theme', theme)

// 初始化消息接收
const initMessageReceiver = () => {
  try {
    window.external.receiveMessage(handleMessage)
  } catch (error) {
    console.error('Failed to initialize message receiver:', error)
  }
}

// 处理接收到的消息
const handleMessage = (message) => {
  try {
    var msg = JSON.parse(message)
    console.log('Received message:', msg)
    if (msg && msg.code == 5) {
      initWindow()
    }
    // 在这里处理接收到的消息
    // 例如：根据消息类型执行不同的操作
  } catch (error) {
    console.error('Failed to handle message:', error)
  }
}


// 窗口初始化
const initWindow = async () => {
  initDragZone()

  showWindowControls.value = true // 窗口控制按钮

  try {
    // 加载涅槃账号信息
    const response = await getNirvanaAccount()
    if (response.code === 1) {
      if (response.data.days < 1) {
        // 跳转到登录页面
        router.push('/login-socket')
      }
    } else {
      router.push('/login-socket')
    }
  } catch (error) {
    console.error('Failed to load nirvana account:', error)
  }

}

const initDragZone = () => {
  const isDragging = ref(false);

  document.addEventListener('mousedown', (e) => {
    if (e.target.nodeName != 'DIV' && e.target.nodeName != 'MAIN' && e.target.nodeName != 'NAV') {
      return;
    }
    isDragging.value = true;
    // 记录鼠标在窗口内的偏移
    // 获取当前窗口位置
    sendMessage('window:drag-start');
  });

  document.addEventListener('mousemove', (e) => {
    if (!isDragging.value) {
      return;
    }
    // 发送绝对屏幕坐标，让后端计算
    sendMessage('window:drag-move', {
      sx: e.screenX,
      sy: e.screenY
    });
  });

  document.addEventListener('mouseup', () => {
    isDragging.value = false;
  });
}

</script>

<template>
  <div class="app" :class="theme">
    <!-- 窗口控制按钮 -->
    <div class="container" :class="{ 'full-width': !showNavbar }">
      <div class="window-controls" v-if="showWindowControls">
        <button class="window-btn minimize-btn" @click="handleMinimize" @mouseenter="showMinimizeIcon = true"
          @mouseleave="showMinimizeIcon = false">
          {{ showMinimizeIcon ? '-' : '' }}
        </button>
        <button class="window-btn close-btn" @click="handleClose" @mouseenter="showCloseIcon = true"
          @mouseleave="showCloseIcon = false">
          {{ showCloseIcon ? '×' : '' }}
        </button>
      </div>
      <!-- 左侧导航栏 -->
      <nav class="sidebar" v-if="showNavbar">
        <div class="logo">
          <h2 class="divider">Fantnel</h2>
          <router-link to="/user" v-if="account">{{ account }}</router-link>
          <router-link to="/login" v-else>点我登录</router-link>
        </div>
        <ul class="nav-list">
          <li>
            <router-link to="/" active-class="active" class="divider">主页</router-link>
          </li>
          <li>
            <router-link to="/game-accounts" active-class="active">游戏账号</router-link>
          </li>
          <li>
            <router-link to="/servers" active-class="active">网络器</router-link>
          </li>
          <li>
            <router-link to="/game-rental" active-class="active">租赁服</router-link>
          </li>
          <li>
            <router-link to="/skins" active-class="active" class="divider">我的皮肤</router-link>
          </li>
          <li>
            <router-link to="/plugins" active-class="active">插件管理</router-link>
          </li>
          <li>
            <router-link to="/plugin-store" active-class="active" class="divider">插件商城</router-link>
          </li>
          <li>
            <router-link to="/proxy-manager" active-class="active">代理管理</router-link>
          </li>
          <li>
            <router-link to="/game-manager" active-class="active" class="divider">游戏管理</router-link>
          </li>
          <li>
            <router-link to="/settings" active-class="active">系统设置</router-link>
          </li>
          <li>
            <router-link to="/logs" active-class="active">日志信息</router-link>
          </li>
        </ul>

        <!-- 主题切换 -->
        <div class="theme-switch">
          <button @click="toggleTheme" class="theme-btn">
            {{ theme === 'light' ? '🌙' : theme === 'dark' ? '☀️' : '⚫' }}
          </button>
          <span class="theme-text">{{ theme === 'light' ? '浅色' : theme === 'dark' ? '深色' : '灰色' }}</span>
        </div>
      </nav>

      <!-- 主内容区 -->
      <main class="main-content">
        <router-view />
      </main>
    </div>

    <!-- 页脚 -->
    <div class="footer" v-if="showNavbar">
      <div class="footer-content">
        <p>© 涅槃科技 2020/11/2 - 至今. 保留所有权利.</p>
        <!-- <p>备案号: 123456</p> -->
      </div>
    </div>

  </div>
</template>

<style>
/* 全局样式重置 */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: Arial, sans-serif;
  transition: background-color 0.3s, color 0.3s;
}

/* 主题样式 */
.app.light {
  --bg-color: #ffffff;
  --text-color: #333333;
  --sidebar-bg: #f5f5f5;
  --sidebar-active: #2196F3;
  --border-color: #dddddd;
  --ad-bg: #f0f0f0;
}

.app.dark {
  --bg-color: #1a1a1a;
  --text-color: #ffffff;
  --sidebar-bg: #2d2d2d;
  --sidebar-active: #4285F4;
  --border-color: #444444;
  --ad-bg: #3d3d3d;
}

.app.gray {
  --bg-color: #2d2d2d;
  --text-color: #cccccc;
  --sidebar-bg: #3d3d3d;
  --sidebar-active: #4285F4;
  --border-color: #444444;
  --ad-bg: #4d4d4d;
}

.app {
  background-color: var(--bg-color);
  color: var(--text-color);
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  position: relative;
}

/* 窗口控制按钮 */
.window-controls {
  position: absolute;
  top: 10px;
  right: 10px;
  display: flex;
  gap: 8px;
  z-index: 1000;
}

.window-btn {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: bold;
  transition: all 0.2s ease;
}

.minimize-btn {
  background-color: #4cd964;
  color: var(--bg-color);
}

.close-btn {
  background-color: #ff3b30;
  color: var(--bg-color);
}

.window-btn:hover {
  transform: scale(1.1);
}

/* 确保容器内容不被窗口控制按钮遮挡 */
.container {
  flex: 1;
  display: flex;
}

/* 左侧导航栏 */
.sidebar {
  width: 200px;
  background-color: var(--sidebar-bg);
  border-right: 1px solid var(--border-color);
  padding: 20px 0;
}

.logo {
  padding: 0 20px 20px;
  border-bottom: 1px solid var(--border-color);
}

.logo h1 {
  font-size: 20px;
  color: var(--text-color);
}

.logo a {
  color: var(--text-color);
}

.nav-list li {
  margin: 5px 0;
}

.nav-list a {
  display: block;
  padding: 10px 20px;
  color: var(--text-color);
  transition: background-color 0.2s;
}

.nav-list a:hover {
  background-color: rgba(33, 150, 243, 0.1);
}

.nav-list a.active {
  background-color: var(--sidebar-active);
  color: white;
}

/* 主题切换 */
.theme-switch {
  margin-top: 8%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 15px;
  padding-right: 24%;
  border-top: 1px solid var(--border-color);
  border-bottom: 1px solid var(--border-color);
}

/* 分割线 */
.divider {
  border-bottom: 1px solid var(--border-color);
}



.theme-btn {
  background: none;
  border: none;
  font-size: 20px;
  cursor: pointer;
  padding: 5px;
  border-radius: 50%;
  transition: background-color 0.2s;
}

.theme-btn:hover {
  background-color: rgba(0, 0, 0, 0.1);
}

.theme-text {
  font-size: 14px;
  color: var(--text-color);
}

/* 主内容区 */
.main-content {
  flex: 1;
  padding: 20px;
  position: relative;
  overflow-y: auto;
  height: 100%;
}

/* 当导航栏隐藏时，容器占满宽度 */
.container.full-width .main-content {
  width: 100%;
}

/* 页脚 */
.footer {
  background-color: var(--sidebar-bg);
  border-top: 1px solid var(--border-color);
  padding: 10px 20px;
  font-size: 12px;
  color: var(--text-color);
}

.footer-content {
  display: flex;
  justify-content: center;
  gap: 20px;
}
</style>
