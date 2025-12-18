<template>
  <div class="plugins">
    <h1>插件管理</h1>
    <div class="plugin-list">
      <table>
        <thead>
          <tr>
            <th>名称</th>
            <th>版本</th>
            <th>作者</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="plugin in plugins" :key="plugin.id">
            <td>{{ plugin.name }}</td>
            <td>{{ plugin.version }}</td>
            <td>{{ plugin.author }}</td>
            <td>
              <span :class="{ 'status-active': plugin.status == 1, 'status-inactive': plugin.status == 0 }">
                {{ plugin.status == 1 ? '启动' : '未启动' }}
              </span>
            </td>
            <td>
              <button @click="togglePluginStatus(plugin.id)" class="toggle-btn">
                {{ plugin.status == 1 ? '停止' : '启动' }}
              </button>
              <button @click="deletePlugin(plugin.id)" class="delete-btn">删除</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- 删除确认对话框 -->
  <Alert
    :show="showDeleteConfirm"
    message="确定要删除这个插件吗？此操作不可恢复。"
    title="删除确认"
    :showCancel="true"
    okText="确认删除"
    cancelText="取消"
    @ok="handleConfirmDelete"
    @cancel="handleCancelDelete"
  />
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getPlugins, togglePluginStatus as togglePluginStatusApi, deletePlugin as deletePluginApi } from '../utils/Tools';
import Alert from '../components/Alert.vue';

const plugins = ref();

// 删除确认对话框相关状态
const showDeleteConfirm = ref(false);
const pluginToDelete = ref(null);

onMounted(async () => {
  plugins.value = await getPlugins().then(res => res.data);
});

// [
//   { id: 1, name: '插件1', version: '1.0.0', author: '作者1', status: true },
//   { id: 2, name: '插件2', version: '2.1.0', author: '作者2', status: false },
//   { id: 3, name: '插件3', version: '0.9.5', author: '作者3', status: true }
// ]

function togglePluginStatus(id) {
  togglePluginStatusApi(id);
  location.reload();
  // const plugin = plugins.value.find(p => p.id === id)
  // if (plugin) {
    // plugin.status = plugin.status == 1 ? 0 : 1
 // }
}

function deletePlugin(id) {
  deletePluginApi(id);
  location.reload();
  // pluginToDelete.value = id;
  // showDeleteConfirm.value = true;
}

// 确认删除插件
function handleConfirmDelete() {
  if (pluginToDelete.value) {
    plugins.value = plugins.value.filter(p => p.id !== pluginToDelete.value);
    pluginToDelete.value = null;
    showDeleteConfirm.value = false;
  }
}

// 取消删除操作
function handleCancelDelete() {
  pluginToDelete.value = null;
  showDeleteConfirm.value = false;
}
</script>

<style scoped>
.plugins {
  padding: 20px;
}

.plugin-list {
  margin-top: 20px;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

th {
  background-color: var(--sidebar-bg);
}

.status-active {
  color: #4CAF50;
  font-weight: bold;
}

.status-inactive {
  color: #f44336;
  font-weight: bold;
}

.toggle-btn, .delete-btn {
  padding: 5px 10px;
  margin-right: 5px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}

.toggle-btn {
  background-color: #2196F3;
  color: white;
}

.delete-btn {
  background-color: #f44336;
  color: white;
}
</style>