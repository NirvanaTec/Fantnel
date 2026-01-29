<template>

  <div class="game-accounts">
    <h1>游戏账号管理</h1>
    <p>游戏账号管理页面，用于管理系统中的游戏账号。</p>
    <div class="account-list">
      <table>
        <thead>
          <tr>
            <th>名称</th>
            <th>账号</th>
            <th>账号类型</th>
            <th>User ID</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="account in accounts">
            <td>{{ account.name }}</td>
            <td>{{ account.account }}</td>
            <td>{{ account.type }}</td>
            <td>{{ account.userId }}</td>
            <td>
              <button @click.stop="selectAccount1(account.id)" class="select-btn">登录</button>
              <button @click.stop="editAccount(account)" class="edit-btn">编辑</button>
              <button @click.stop="deleteAccount1(account.id)" class="delete-btn">删除</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 编辑账号弹窗 -->
    <div v-if="showEditModal" class="modal">
      <div class="modal-content">
        <h2>编辑账号</h2>

        <!-- 错误信息显示 -->
        <div v-if="error" class="error-message">
          {{ error }}
        </div>

        <form @submit="saveAccount">
          <div class="form-group">
            <label>名称:</label>
            <input v-model="editingAccount.name" type="text" required>
          </div>
          <div class="form-group" v-if="editingAccount.type === 'cookie'">
            <label>Cookie/Auth:</label>
            <textarea v-model="editingAccount.password" rows="4" required></textarea>
          </div>
          <div class="form-group" v-else>
            <label>账号:</label>
            <input v-model="editingAccount.account" type="text" required>
          </div>
          <div class="form-group" v-if="editingAccount.type !== 'cookie'">
            <label>密码:</label>
            <input v-model="editingAccount.password" type="text" required>
          </div>
          <div class="form-group">
            <label>账号类型:</label>
            <select v-model="editingAccount.type">
              <option value="4399">4399</option>
              <option value="163">163</option>
              <option value="cookie">cookie/auth</option>
            </select>
          </div>
          <div class="modal-actions">
            <button type="submit" class="save-btn" :disabled="loading">
              {{ loading ? '保存中...' : '保存' }}
            </button>
            <button type="button" @click="showEditModal = false" class="cancel-btn" :disabled="loading">取消</button>
          </div>
        </form>
      </div>
    </div>

    <!-- 添加账号弹窗 -->
    <div v-if="showAddModal" class="modal">
      <div class="modal-content">
        <h2>添加账号</h2>

        <!-- 错误信息显示 -->
        <div v-if="error" class="error-message">
          {{ error }}
        </div>

        <form @submit="addNewAccount">
          <div class="form-group">
            <label>名称:</label>
            <input v-model="newAccount.name" type="text" required>
          </div>
          <div class="form-group" v-if="newAccount.type === 'cookie'">
            <label>Cookie/Auth:</label>
            <textarea v-model="newAccount.password" rows="4" required></textarea>
          </div>
          <div class="form-group" v-else>
            <label>账号:</label>
            <input v-model="newAccount.account" type="text" required>
          </div>
          <div class="form-group" v-if="newAccount.type !== 'cookie'">
            <label>密码:</label>
            <input v-model="newAccount.password" type="text" required>
          </div>
          <div class="form-group">
            <label>账号类型:</label>
            <select v-model="newAccount.type">
              <option value="4399">4399</option>
              <!-- <option value="163">163</option> -->
              <option value="cookie">cookie/auth</option>
            </select>
          </div>
          <div class="modal-actions">
            <button type="submit" class="save-btn" :disabled="loading">
              {{ loading ? '添加中...' : '添加' }}
            </button>
            <button type="button" @click="showAddModal = false" class="cancel-btn" :disabled="loading">取消</button>
          </div>
        </form>
      </div>
    </div>

    <button @click="showAddModal = true" class="add-btn">添加账号</button>

    <!-- 错误信息显示 -->
    <div v-if="error" class="error-message">
      {{ error }}
    </div>

    <!-- 删除确认对话框 -->
    <Alert :show="showDeleteConfirm" message="确定要删除这个账号吗？删除后将无法恢复。" title="删除确认" :showCancel="true" okText="确认删除"
      cancelText="取消" @ok="handleConfirmDelete" @cancel="handleCancelDelete" />

    <!-- 登录中提示 -->
    <Alert :show="loggingIn" message="登录中，请稍候..." title="登录中" :closable="false" :showCancel="false" />

    <!-- 验证码输入弹窗 -->
    <div v-if="showCaptchaModal" class="modal">
      <div class="modal-content">
        <h2>验证码登录</h2>

        <!-- 验证码错误信息显示 -->
        <div v-if="captchaError" class="error-message">
          {{ captchaError }}
        </div>

        <div class="captcha-container">
          <div class="captcha-image-container">
            <img v-if="captchaImage" :src="captchaImage" alt="验证码" class="captcha-image" @click="loadCaptcha">
            <div v-else-if="captchaLoading" class="captcha-loading">加载中...</div>
            <div v-else class="captcha-error" @click="loadCaptcha">无法加载验证码</div>
          </div>

          <div class="captcha-hint">点击验证码图片可刷新</div>

          <div class="form-group">
            <label>验证码:</label>
            <div class="captcha-input-group">
              <input v-model="captchaInput" type="text" placeholder="请输入验证码" required>
            </div>
          </div>
        </div>

        <div class="modal-actions">
          <button type="button" @click="submitCaptcha" class="save-btn" :disabled="captchaLoading">
            {{ captchaLoading ? '验证中...' : '确认登录' }}
          </button>
          <button type="button" @click="closeCaptchaModal" class="cancel-btn" :disabled="captchaLoading">取消</button>
        </div>
      </div>
    </div>
  </div>

</template>

<script setup>
import { ref } from 'vue'
import { getAccounts, addAccount, selectAccount, deleteAccount, updateAccount, getCaptcha4399Url, verifyCaptcha4399, getCaptcha4399Content } from '../utils/Tools'
import Alert from '../components/Alert.vue'

const accounts = ref([])
const loading = ref(false)
const error = ref('')

// 删除确认对话框相关状态
const showDeleteConfirm = ref(false)
const accountToDelete = ref(null)

getAccounts().then(data => {
  accounts.value = data.data || []
}).catch((err) => {
  if (err.status === 500) {
    error.value = '连接服务器失败，请检查应用状态。'
  } else {
    error.value = err.message || '获取账号列表失败，请稍后重试'
  }
})

const showEditModal = ref(false)
const showAddModal = ref(false)
const showCaptchaModal = ref(false)
const editingAccount = ref({ id: null, name: null, account: null, password: null, type: null, userId: null })
const newAccount = ref({ name: null, account: null, password: null, type: "4399" })

// 验证码相关状态
const captchaImage = ref(null);
const captchaInput = ref('')
const accountToLogin = ref(null)
const captchaLoading = ref(false)

// 登录中状态
const loggingIn = ref(false)

function selectAccount1(id) {
  const account = accounts.value.find(acc => acc.id === id);
  if (account && account.type === '4399') {
    // 4399账号需要验证码
    accountToLogin.value = id;
    if (!captchaImage.value) {
      loadCaptcha();
    }
    showCaptchaModal.value = true;
  } else {
    selectAccount2(id);
  }
}

function selectAccount2(id) {
  // 显示登录中提示
  loggingIn.value = true;

  // 非4399账号直接登录
  selectAccount(id).then(data => {
    if (data.code === 1) {
      location.reload();
    } else {
      error.value = data.msg || '选择失败，请稍后重试';
    }
  }).catch(err => {
    console.error('选择账号失败:', err);
    error.value = '网络错误，请检查连接后重试';
  }).finally(() => {
    // 隐藏登录中提示
    loggingIn.value = false;
  });
}

// 加载验证码图片
function loadCaptcha() {
  captchaLoading.value = true;
  captchaError.value = '';
  // 添加时间戳防止浏览器缓存
  const timestamp = new Date().getTime();
  getCaptcha4399Url().then(url => {
    const imgUrl = url + '?t=' + timestamp;
    captchaImage.value = imgUrl;

    // 创建临时图片对象监听加载完成事件
    const img = new Image();
    img.onload = () => {
      // 确保图片加载完成
      captchaLoading.value = false;
      // 获取验证码内容
      getCaptcha4399Content().then(data => {
        if (data.code === 1) {
          captchaInput.value = data.data || '';
        } else {
          captchaInput.value = '';
        }
      })
    };
    img.onerror = () => {
      // 图片加载失败也需要更新状态
      captchaLoading.value = false;
      captchaError.value = '验证码图片加载失败，请点击重试';
    };
    img.src = imgUrl;
  });
}

// 验证码错误信息
const captchaError = ref('');

// 提交验证码
function submitCaptcha() {
  if (!captchaInput.value) {
    captchaError.value = '请输入验证码';
    return;
  }

  captchaLoading.value = true;
  captchaError.value = '';

  // 准备验证参数
  const verifyData = {
    id: accountToLogin.value,
    captcha: captchaInput.value
  };

  verifyCaptcha4399(verifyData).then(data => {
    if (data.code === 1) {
      // 验证码验证成功，执行登录
      showCaptchaModal.value = false;
      selectAccount2(accountToLogin.value);
    } else {
      // 验证码验证失败，显示错误信息并刷新验证码
      captchaError.value = data.msg || '验证码错误，请重试';
      loadCaptcha();
    }
  }).catch(err => {
    console.error('验证码验证失败:', err);
    captchaError.value = err.message || '操作失败，请重试';
    loadCaptcha();
  }).finally(() => {
    captchaLoading.value = false;
  });
}

// 关闭验证码模态框
function closeCaptchaModal() {
  showCaptchaModal.value = false;
  if (captchaInput.value.length > 10) {
    captchaInput.value = '';
  }
  captchaError.value = '';
}

function editAccount(account) {
  editingAccount.value = { ...account }
  showEditModal.value = true
}

function saveAccount(event) {
  event.preventDefault();

  // 重置错误信息
  error.value = '';

  // 表单验证
  if (!editingAccount.value.name || !editingAccount.value.password) {
    error.value = '请填写所有基本信息';
    return;
  }
  // Cookie类型账号不需要验证account字段
  if (editingAccount.value.type !== 'cookie' && !editingAccount.value.account) {
    error.value = '请填写所有必填字段';
    return;
  }

  loading.value = true;

  updateAccount(editingAccount.value).then(data => {
    if (data.code === 1) {
      location.reload();
      // showEditModal.value = false;
      // loading.value = false;
    } else {
      error.value = data.msg || '更新失败，请稍后重试';
      loading.value = false;
    }
  }).catch(err => {
    console.error('更新账号失败:', err);
    error.value = '网络错误，请检查连接后重试';
    loading.value = false;
  });

}

function deleteAccount1(id) {
  // 显示删除确认对话框
  accountToDelete.value = id;
  showDeleteConfirm.value = true;
}

// 确认删除账号
function handleConfirmDelete() {
  deleteAccount(accountToDelete.value).then(data => {
    if (data.code === 1) {
      location.reload();
    } else {
      error.value = data.msg || '删除失败，请稍后重试';
    }
  }).catch(err => {
    console.error('删除账号失败:', err);
    error.value = '网络错误，请检查连接后重试';
  }).finally(() => {
    accountToDelete.value = null;
    showDeleteConfirm.value = false;
  });
}

// 取消删除操作
function handleCancelDelete() {
  accountToDelete.value = null;
  showDeleteConfirm.value = false;
}

function addNewAccount(event) {
  event.preventDefault();

  // 重置错误信息
  error.value = '';

  // 表单验证
  if (!newAccount.value.name || !newAccount.value.password) {
    error.value = '请填写所有基本信息';
    return;
  }
  // Cookie类型账号不需要验证account字段
  if (newAccount.value.type !== 'cookie' && !newAccount.value.account) {
    error.value = '请填写所有必填字段';
    return;
  }

  loading.value = true;

  if (newAccount.value.type === 'cookie') {
    newAccount.value.account = null;
  }

  addAccount(newAccount.value).then(data => {
    if (data.code === 1) {
      location.reload();
      // showAddModal.value = false;
      // newAccount.value = { name: '', account: '', password: '', type: '4399' };
    } else {
      error.value = data.msg || '添加失败，请稍后重试';
    }
  }).catch(err => {
    console.error('添加账号失败:', err);
    error.value = '网络错误，请检查连接后重试';
  }).finally(() => {
    loading.value = false;
  });
}

</script>

<style scoped>
.game-accounts {
  padding: 20px;
}

.account-list {
  margin-top: 20px;
  position: relative;
  display: inline-block;
  width: 100%;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

th {
  background-color: var(--sidebar-bg);
}

tr:hover {
  background-color: rgba(0, 0, 0, 0.05);
}

.edit-btn,
.select-btn,
.delete-btn {
  padding: 5px 10px;
  margin-right: 5px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}

.select-btn {
  background-color: #2196F3;
  color: white;
}

.edit-btn {
  background-color: #4CAF50;
  color: white;
}

.delete-btn {
  background-color: #f44336;
  color: white;
}

.add-btn {
  position: fixed;
  right: 20px;
  bottom: 50px;
  background-color: #2196F3;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 5px;
  font-size: 16px;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  z-index: 5;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background-color: var(--sidebar-bg);
  color: var(--text-color);
  padding: 20px;
  border-radius: 5px;
  width: 400px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  color: var(--text-color);
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid var(--border-color);
  border-radius: 3px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--sidebar-active);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.save-btn,
.cancel-btn {
  padding: 8px 15px;
  margin-left: 10px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-weight: 500;
  transition: opacity 0.3s;
}

.save-btn {
  background-color: #4CAF50;
  color: white;
}

.cancel-btn {
  background-color: #f44336;
  color: white;
}

.save-btn:disabled,
.cancel-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error-message {
  color: #f44336;
  background-color: #ffebee;
  padding: 10px;
  border-radius: 3px;
  margin-bottom: 15px;
  font-size: 14px;
}

/* 验证码相关样式 */
.captcha-container {
  margin-top: 20px;
}

.captcha-input-group {
  display: flex;
  gap: 10px;
}

.captcha-image-container {
  margin: 15px 0;
  text-align: center;
  cursor: pointer;
}

.captcha-image {
  width: 100px;
  height: 50px;
  object-fit: cover;
  border: 1px solid var(--border-color);
  border-radius: 3px;
  cursor: pointer;
  user-select: none;
}

.captcha-image:hover {
  border-color: var(--sidebar-active);
}

.captcha-loading,
.captcha-error {
  padding: 20px;
  background-color: var(--bg-color);
  border: 1px solid var(--border-color);
  border-radius: 3px;
  color: var(--text-color);
}

.captcha-hint {
  text-align: center;
  font-size: 12px;
  color: var(--text-color);
  opacity: 0.7;
  margin-top: 5px;
}
</style>