<script setup>
import { ref, provide } from 'vue'

const theme = ref('light')

function toggleTheme() {
  theme.value = theme.value === 'light' ? 'dark' : theme.value === 'dark' ? 'gray' : 'light'
}

provide('theme', theme)
</script>

<template>
  <div class="app" :class="theme">
    <div class="container">
      <!-- 左侧导航栏 -->
      <nav class="sidebar">
        <div class="logo">
          <h1>Fantnel</h1>
        </div>
        <ul class="nav-list">
          <li>
            <router-link to="/" active-class="active">主页</router-link>
          </li>
          <li>
            <router-link to="/game-accounts" active-class="active">游戏账号</router-link>
          </li>
          <li>
            <router-link to="/servers" active-class="active">服务器</router-link>
          </li>
          <li>
            <router-link to="/plugins" active-class="active">插件管理</router-link>
          </li>
          <li>
            <router-link to="/plugin-store" active-class="active">插件商城</router-link>
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
    <div class="footer">
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
}

.container {
  display: flex;
  flex: 1;
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
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 15px;
  padding-right: 24%;
  border-top: 1px solid var(--border-color);
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
