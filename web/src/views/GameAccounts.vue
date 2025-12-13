<template>

  <div class="game-accounts">
    <h1>游戏账号管理</h1>
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
            <input v-model="editingAccount.password" type="password" required>
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
            <input v-model="newAccount.password" type="password" required>
          </div>
          <div class="form-group">
            <label>账号类型:</label>
            <select v-model="newAccount.type">
              <option value="4399">4399</option>
              <option value="163">163</option>
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
    <Alert
      :show="showDeleteConfirm"
      message="确定要删除这个账号吗？删除后将无法恢复。"
      title="删除确认"
      :showCancel="true"
      okText="确认删除"
      cancelText="取消"
      @ok="handleConfirmDelete"
      @cancel="handleCancelDelete"
    />
  </div>

</template>

<script setup>
import { ref } from 'vue'
import { getAccounts, addAccount, selectAccount, deleteAccount, updateAccount } from '../utils/Tools'
import Alert from '../components/Alert.vue'

const accounts = ref([])
const loading = ref(false)
const error = ref('')

// 删除确认对话框相关状态
const showDeleteConfirm = ref(false)
const accountToDelete = ref(null)

getAccounts().then(data => {
  accounts.value = data.data || []
})

const showEditModal = ref(false)
const showAddModal = ref(false)
const editingAccount = ref({ id: null, name: null, account: null, password: null, type: null, userId: null })
const newAccount = ref({ name: null, account: null, password: null, type: "4399" })

function selectAccount1(id) {
  selectAccount(id).then(data => {
    if (data.code === 1) {
      location.reload();
    } else {
      error.value = data.msg || '选择失败，请稍后重试';
    }
  }).catch(err => {
    console.error('选择账号失败:', err);
    error.value = '网络错误，请检查连接后重试';
  });
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
  if (!editingAccount.value.name || !editingAccount.value.account || !editingAccount.value.password) {
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
  if (accountToDelete.value) {
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
  if (!newAccount.value.name || !newAccount.value.account || !newAccount.value.password) {
    error.value = '请填写所有必填字段';
    return;
  }

  loading.value = true;

  if(newAccount.value.type === 'cookie'){
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
</style>