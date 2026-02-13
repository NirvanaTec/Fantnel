<template>
  <div class="login-page">
    <div class="login-container">
      <!-- 标题 -->
      <div class="login-header">
        <h1 class="login-title">Fantnel</h1>
        <p class="login-subtitle">请登录您的账号</p>
      </div>
      
      <!-- 登录表单 -->
      <div class="login-form">
        <form @submit.prevent="toLogin">
            <div class="form-group">
                <label class="form-label" for="Account">账号</label>
                <input v-model="account"
                    class="form-input"
                    id="Account" placeholder="请输入您的账号" type="text" required>
            </div>

            <div class="form-group">
                <label class="form-label" for="Password">密码</label>
                <input v-model="password"
                    class="form-input"
                    id="Password" placeholder="请输入您的密码" type="password" required>
            </div>

            <button
                class="login-button"
                type="submit">
                登录
            </button>

            <div class="login-footer">
                <span class="copyright">&copy; 涅槃科技</span>
                <div class="support-links">
                    <a class="support-link" href="http://npyyds.top/" target="_block" >注册账号</a>
                    <a class="support-link" href="http://npyyds.top/shop" target="_block" >涅槃商城</a>
                </div>
            </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { loginNirvana, getNirvanaAccount } from '../../utils/Tools.js'
import { Message } from '../../utils/message.js'

const account = ref('')
const password = ref('')

const toLogin = async () => {
    try {
        const data = await loginNirvana(account.value, password.value);
        if(data.code === 1){
            Message.success(data.msg || '登录成功')
            // 登录成功后跳转至用户首页
            window.location.href = '/user'
        }else{
            Message.warning(data.msg || '登录失败')
        }
    } catch (error) {
        Message.error('登录失败，请检查网络连接')
    }
}

onMounted(() => {
    // 页面加载时检查是否已登录
    getNirvanaAccount().then(data => {
        if (data.code === 1) {
            // 如果已登录，直接跳转至用户首页
            window.location.href = '/user'
        }
    })
})


</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.login-container {
  width: 100%;
  max-width: 490px;
  background-color: var(--sidebar-bg);
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  padding: 40px;
  border: 1px solid var(--border-color);
  transform: translateY(-10%);
}

.login-header {
  text-align: center;
  margin-bottom: 30px;
}

.login-title {
  font-size: 1.8rem;
  font-weight: bold;
  color: var(--text-color);
  margin-bottom: 8px;
}

.login-subtitle {
  font-size: 1rem;
  color: var(--text-color);
  opacity: 0.8;
}

.login-form {
  width: 100%;
}

.form-group {
  margin-bottom: 30px;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  color: var(--text-color);
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background-color: var(--bg-color);
  color: var(--text-color);
  font-size: 1rem;
  transition: all 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: var(--sidebar-active);
  box-shadow: 0 0 0 2px rgba(66, 133, 244, 0.1);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
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

.forgot-password {
  font-size: 0.9rem;
  color: var(--sidebar-active);
  text-decoration: none;
  transition: opacity 0.3s ease;
}

.forgot-password:hover {
  opacity: 0.8;
}

.login-button {
  width: 100%;
  padding: 14px;
  background-color: var(--sidebar-active);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.login-button:hover {
  background-color: rgba(66, 133, 244, 0.9);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(66, 133, 244, 0.3);
}

.login-button:active {
  transform: translateY(0);
}

.login-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid var(--border-color);
}

.copyright {
  font-size: 0.85rem;
  color: var(--text-color);
  opacity: 0.6;
}

.support-links {
  display: flex;
  gap: 20px;
}

.support-link {
  font-size: 0.85rem;
  color: var(--text-color);
  opacity: 0.6;
  text-decoration: none;
  transition: opacity 0.3s ease;
}

.support-link:hover {
  opacity: 1;
}



/* 响应式设计 */
@media (max-width: 480px) {
  .login-container {
    padding: 30px 20px;
  }
  
  .login-title {
    font-size: 1.5rem;
  }
  
  .form-input {
    padding: 10px 14px;
  }
  
  .login-button {
    padding: 12px;
  }
}
</style>