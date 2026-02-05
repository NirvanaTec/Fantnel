<template>
  <div class="server-detail">
    <div class="server-header">
      <div class="server-title">
        <h1>{{ server.name }}</h1>
        <p>服务器ID: {{ server.id }}</p>
      </div>
      <button class="launch-btn" @click="showLaunchModal = true">Launch</button>
    </div>

    <div class="server-images">
      <img :src="mainImage" alt="服务器介绍图" class="main-image" width="584" height="329">
      <div class="small-images">
        <img v-for="(img, index) in server.brief_image_urls" :key="index" :src="img" alt="小介绍图" class="small-image"
          width="80" height="48" @click="switchImage(img)">
      </div>
    </div>

    <div class="server-meta">
      <div class="meta-item">
        <span class="label">服务器作者:</span>
        <span class="value">{{ server.author }}</span>
      </div>
      <div class="meta-item">
        <span class="label">创建时间:</span>
        <span class="value">{{ server.createdAt }}</span>
      </div>
      <div class="meta-item">
        <span class="label">游戏版本:</span>
        <span class="value">{{ server.gameVersion }}</span>
      </div>
      <div class="meta-item">
        <span class="label">服务器地址:</span>
        <span class="value">{{ server.address }}</span>
      </div>
    </div>

    <div class="server-description">
      <h2>服务器介绍</h2>
      <p v-html="server.fullDescription"></p>
    </div>

    <!-- Launch 弹窗 -->
    <div v-if="showLaunchModal" class="modal">
      <div class="modal-content">
        <h2>Launch 游戏</h2>
        <b>
          <h6>出现问题可以前往论坛反馈哦~</h6>
        </b>
        <form @submit.prevent="launchGameBtn">
          <div class="form-group">
            <label><b>账号: <h6 style="display: inline;">登录成功后的账号才会显示在账号列表中。</h6></b></label>
            <select v-model="selectedAccount" @change="selectAccount1">
              <option v-for="account in accounts" :key="account.id" :value="account.id">{{ account.name }}</option>
            </select>

          </div>
          <div class="form-group">
            <label><b>游戏名称: <h6 style="display: inline;">请先 添加 / 选择 游戏名称，才能启动。</h6></b></label>
            <div class="select-with-add" v-if="games.length > 0">
              <select v-model="selectedGame">
                <option v-for="game in games" :key="game.id" :value="game.name">{{ game.name }}</option>
              </select>
            </div>
            <div v-if="showAddGameInput" class="add-game-input">
              <input v-model="newGameName" type="text" placeholder="输入新游戏名称">
              <button type="button" class="save-game-btn" @click="addNewGame">保存</button>
            </div>
          </div>
          <div class="modal-actions">
            <button type="button" class="add-game-btn" @click="showAddGameInput = !showAddGameInput">
              {{ showAddGameInput ? '取消添加' : '添加名称' }}
            </button>
            <button type="submit" class="launch-game-btn">启动游戏</button>
            <button type="button" @click="launchProxyBtn" class="launch-proxy-btn">启动代理</button>
            <button type="button" @click="showLaunchModal = false" class="cancel-btn">取消</button>
          </div>
        </form>
      </div>
    </div>

    <!-- 代理弹窗 -->
    <div v-if="showProxyModal" class="modal">
      <div class="modal-content">
        <h2>启动代理</h2>
        <b>
          <h6>如果你未安装 核心依赖插件 这大概会导致验证失败。</h6>
          <h6>部分服务器可能需要安装 插件 才能正常进入游戏。</h6>
        </b>
        <div class="plugins-section">
          <div v-if="isLoadingPlugins" class="loading">加载中...</div>
          <div v-else-if="pluginError" class="plugin-error">
            <div class="error-message">{{ pluginError }}</div>
          </div>
          <div v-else class="plugins-container">
            <!-- 当有base插件时显示左右对称布局 -->
            <div v-if="basePlugins.length > 0" class="plugins-symmetric">
              <div class="plugin-column">
                <h4>服务器推荐插件：</h4>
                <ul v-if="dependencePlugins.length > 0" class="plugin-list">
                  <li v-for="(plugin, index) in dependencePlugins" :key="index">
                    <a :href="`/plugin/${plugin.id}`" class="plugin-link">{{ plugin.name }}</a>
                  </li>
                </ul>
                <div v-else class="no-plugins">无依赖插件</div>
              </div>
              <div class="plugin-column">
                <h4>核心依赖插件：</h4>
                <ul v-if="basePlugins.length > 0" class="plugin-list">
                  <li v-for="(plugin, index) in basePlugins" :key="index">
                    <a :href="`/plugin/${plugin.id}`" class="plugin-link">{{ plugin.name }}</a>
                  </li>
                </ul>
                <div v-else class="no-plugins">无核心插件</div>
              </div>
            </div>

            <!-- 当没有base插件时只显示dependence插件 -->
            <div v-else>
              <h4>服务器推荐插件：</h4>
              <ul v-if="dependencePlugins.length > 0" class="plugin-list">
                <li v-for="(plugin, index) in dependencePlugins" :key="index">
                  <a :href="`/plugin/${plugin.id}`" class="plugin-link">{{ plugin.name }}</a>
                </li>
              </ul>
              <div v-else class="no-plugins">无依赖插件</div>
            </div>
          </div>
        </div>
        <form @submit.prevent="launchProxyConfirm">
          <div class="modal-actions">
            <button type="submit" class="launch-game-btn">启动代理</button>
            <button type="button" @click="showProxyModal = false" class="cancel-btn">取消</button>
          </div>
        </form>
      </div>
    </div>

    <!-- 使用Alert组件 -->
    <Alert :show="showNotice" :message="noticeText" title="注意事项" :location="noticeLocation" @ok="handleNoticeOk"
      @close="showNotice = false" />
  </div>
</template>

<script setup>
import { ref, onMounted, watchEffect } from 'vue'
import { useRoute } from 'vue-router'
import { selectServer, getServerInfo, addServerRole, launchGame, switchAccount, getGameAccount, launchProxy, getServerPlugins } from '../utils/Tools'
import Alert from '../components/Alert.vue'
import randomNameData from '../../public/random.name.json'

const route = useRoute()
const serverId = route.params.id

const server = ref({})

// {
//   id: serverId,
//   name: `服务器${serverId}`,
//   author: '作者名称',
//   createdAt: '2023-01-01',
//   gameVersion: '1.19.2',
//   address: 'mc.example.com:25565',
//   brief_image_urls: [
//     'https://via.placeholder.com/80x48',
//     'https://via.placeholder.com/80x48',
//     'https://via.placeholder.com/80x48'
//   ],
//   fullDescription: '这是一个详细的服务器介绍，包含了服务器的各种特色和玩法。服务器拥有丰富的插件和活动，欢迎玩家加入体验。' + ' '.repeat(100) // 填充到500字左右
// }

const mainImage = ref("");

const showLaunchModal = ref(false);
const selectedAccount = ref('');

const accounts = ref()
// [
//   { id: 1, name: '账号1' },
//   { id: 2, name: '账号2' }
// ]

const games = ref()
// [
//   { id: 1, name: '游戏1' },
//   { id: 2, name: '游戏2' }
// ]

const showAddGameInput = ref(false)
const newGameName = ref('') // 新添加的游戏名称
const selectedGame = ref('')

// 生成随机游戏名称
function generateRandomGameName() {
  const { prefixes, suffixes } = randomNameData
  let result, randomPrefix, randomSuffix, randomNumber

  do {
    randomPrefix = prefixes[Math.floor(Math.random() * prefixes.length)]
    randomSuffix = suffixes[Math.floor(Math.random() * suffixes.length)]
    randomNumber = ''

    // 先组合前缀和后缀
    let baseName = `${randomPrefix}${randomSuffix}`

    // 计算需要的随机数长度
    const currentLength = baseName.length
    if (currentLength < 7) {
      // 需要添加随机数，确保总长度在7-9之间
      const minNumbers = Math.max(0, 7 - currentLength)
      const maxNumbers = Math.max(0, 9 - currentLength)
      const numberLength = Math.floor(Math.random() * (maxNumbers - minNumbers + 1)) + minNumbers
      randomNumber = Math.floor(Math.random() * Math.pow(10, numberLength)).toString().padStart(numberLength, '0')
    } else if (currentLength > 9) {
      // 如果前缀+后缀已经超过9，需要重新选择
      continue
    }

    result = `${baseName}${randomNumber}`
  } while (result.length < 7 || result.length > 9)

  return result
}

// 当显示添加游戏输入框时自动生成随机名称
watchEffect(() => {
  if (showAddGameInput.value) {
    newGameName.value = generateRandomGameName()
  }
})

// 提示框
const showNotice = ref(false)
const noticeText = ref("")
const noticeLocation = ref("")

// 代理弹窗
const showProxyModal = ref(false)
const dependencePlugins = ref([])
const basePlugins = ref([])
const isLoadingPlugins = ref(false)
const pluginError = ref(null)

onMounted(() => {
  selectServer(serverId).then(data => {
    if (data.code !== 1) {
      noticeText.value = data.msg;
      showNotice.value = true;
      noticeLocation.value = "/game-accounts";
      return;
    }
    server.value = data.data;
    mainImage.value = server.value.brief_image_urls[0];
  })
  getServerInfo1();
})

function selectAccount1(event) {
  var account = switchAccount(event.target.value);
  account.then(data => {
    if (data.code === 1) {
      getServerInfo1();
    } else {
      noticeText.value = data.msg;
      showNotice.value = true;
      // 刷新账号选中
      refreshSelectedAccount();
    }
  });
}

function getServerInfo1() {
  getServerInfo(serverId).then(data => {
    accounts.value = data.data.accounts
    games.value = data.data.games

    // 账号默认选中
    refreshSelectedAccount()

    // 名称默认选中
    if (games.value && games.value.length > 0) {
      selectedGame.value = games.value[0].name
    } else {
      showAddGameInput.value = true
    }
  })
}

// 刷新当前选择账号
function refreshSelectedAccount() {
  getGameAccount().then(data => {
    if (data.code === 1) {
      selectedAccount.value = data.data.id;
    }
  });
}

function switchImage(img) {
  mainImage.value = img
}

function launchGameBtn() {
  if (!selectedGame.value) {
    noticeText.value = '请选择游戏名称';
    showNotice.value = true;
    return;
  }
  // alert(`启动游戏: ${selectedGame.value}，账号: ${selectedAccount.value}`)
  showLaunchModal.value = false
  noticeText.value = "正在启动游戏中，请稍后.....";
  launchGame(serverId, selectedGame.value).then(data => {
    // 启动游戏完成，更多信息请查看控制台
    noticeText.value = data.msg;
  }).catch(err => {
    noticeText.value = err.message;
  });
  showNotice.value = true;
}

async function launchProxyBtn() {
  // 加载服务器依赖插件
  await loadServerPlugins();

  // 检查是否有依赖插件（dependence或base）或插件加载错误
  const hasPlugins = dependencePlugins.value.length > 0 || basePlugins.value.length > 0;
  const hasError = pluginError.value !== null;

  if (hasPlugins || hasError) {
    // 有依赖插件或插件加载错误时显示代理弹窗
    showProxyModal.value = true;
  } else {
    // 无依赖插件时直接启动代理
    launchProxyConfirm();
  }
}

function loadServerPlugins() {
  return new Promise((resolve, reject) => {
    isLoadingPlugins.value = true;
    pluginError.value = null;
    getServerPlugins(serverId, server.value.gameVersion).then(data => {
      if (data.code === 1) {
        // 处理插件数据，分离dependence和base模式的插件
        const dependencePlugin = data.data.find(item => item.mode === 'dependence');
        const basePlugin = data.data.find(item => item.mode === 'base');

        dependencePlugins.value = dependencePlugin && dependencePlugin.data ? dependencePlugin.data : [];
        basePlugins.value = basePlugin && basePlugin.data ? basePlugin.data : [];
      } else {
        dependencePlugins.value = [];
        basePlugins.value = [];
        pluginError.value = data.msg;
      }
      isLoadingPlugins.value = false;
      resolve();
    }).catch(err => {
      dependencePlugins.value = [];
      basePlugins.value = [];
      pluginError.value = err.message;
      isLoadingPlugins.value = false;
      resolve(); // 即使出错也resolve，确保流程继续
    });
  });
}

function launchProxyConfirm() {
  if (!selectedGame.value) {
    noticeText.value = '请选择游戏名称';
    showNotice.value = true;
    return;
  }
  // 关闭弹窗
  showProxyModal.value = false;
  noticeText.value = "正在启动代理中，请稍后.....";
  launchProxy(serverId, selectedGame.value).then(data => {
    // 启动代理完成，更多信息请查看控制台
    noticeText.value = data.msg;
  }).catch(err => {
    noticeText.value = err.message;
  });
  showNotice.value = true;
}

function addNewGame() {
  if (newGameName.value.trim()) {
    // const newId = Math.max(...games.value.map(g => g.id), 0) + 1
    // games.value.push({
    //   id: newId,
    //   name: newGameName.value.trim()
    // })
    // selectedGame.value = newGameName.value.trim()
    // newGameName.value = ''
    // showAddGameInput.value = false
    showNotice.value = true;
    noticeText.value = "正在添加角色中，请稍后.....";
    addServerRole(serverId, newGameName.value).then(data => {
      if (data.code === 1) {
        getServerInfo1();
      }
      noticeText.value = data.msg;
    });
  }
}

function handleNoticeOk() {
  showNotice.value = false;
  if (noticeLocation.value && noticeLocation.value !== '') {
    location.href = noticeLocation.value;
  }
}
</script>

<style scoped>
.server-detail {
  padding: 20px;
}

.server-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.server-title h1 {
  margin: 0;
  font-size: 24px;
}

.server-title p {
  margin: 5px 0 0 0;
  color: var(--text-color);
}

.launch-btn {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
}

.server-images {
  margin-bottom: 20px;
}

.main-image {
  width: 100%;
  max-width: 584px;
  height: auto;
  object-fit: cover;
  border-radius: 5px;
  margin-bottom: 10px;
}

.small-images {
  display: flex;
  gap: 10px;
}

.small-image {
  width: 80px;
  height: 48px;
  object-fit: cover;
  border-radius: 3px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: border-color 0.2s;
}

.small-image:hover {
  border-color: #2196F3;
}

.server-meta {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 20px;
  padding: 15px;
  background-color: var(--sidebar-bg);
  border-radius: 5px;
}

.meta-item {
  display: flex;
  flex-direction: column;
}

.meta-item .label {
  font-weight: bold;
  color: var(--text-color);
  font-size: 14px;
}

.meta-item .value {
  margin-top: 5px;
  font-size: 16px;
  color: var(--text-color);
}

.server-description {
  margin-top: 20px;
}

.server-description h2 {
  margin: 0 0 10px 0;
  font-size: 20px;
  color: var(--text-color);
}

.server-description p {
  line-height: 1.6;
  color: var(--text-color);
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
}

.modal-content {
  background-color: var(--sidebar-bg);
  color: var(--text-color);
  padding: 20px;
  border-radius: 5px;
  width: 430px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

.form-group {
  margin-bottom: 15px;
  margin-top: 10px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  color: var(--text-color);
}

.form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid var(--border-color);
  border-radius: 3px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.form-group select:focus {
  outline: none;
  border-color: var(--sidebar-active);
}

.select-with-add {
  display: flex;
  gap: 10px;
  align-items: center;
}

.add-game-btn {
  background-color: var(--sidebar-active);
  color: white;
  border: none;
  border-radius: 3px;
}

.add-game-btn:hover {
  opacity: 0.9;
}

.add-game-input {
  display: flex;
  gap: 10px;
  margin-top: 10px;
}

.add-game-input input {
  flex: 1;
  padding: 8px;
  border: 1px solid var(--border-color);
  border-radius: 3px;
  background-color: var(--bg-color);
  color: var(--text-color);
}

.add-game-input input:focus {
  outline: none;
  border-color: var(--sidebar-active);
}

.save-game-btn {
  padding: 8px 12px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-size: 14px;
}

.save-game-btn:hover {
  opacity: 0.9;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
  gap: 10px;
}

.launch-game-btn,
.launch-proxy-btn,
.cancel-btn {
  padding: 8px 15px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
}

.launch-game-btn {
  background-color: #4CAF50;
  color: white;
}

.launch-proxy-btn {
  background-color: #2196F3;
  color: white;
}

.cancel-btn {
  background-color: #f44336;
  color: white;
}

/* 插件列表样式 */
.plugins-section {
  margin: 15px 0;
  padding: 10px;
  background-color: var(--sidebar-bg);
  border-radius: 5px;
}

.plugins-section h4 {
  margin: 0 0 10px 0;
  color: var(--text-color);
  font-size: 14px;
}

.plugins-container {
  width: 100%;
}

/* 对称布局样式 */
.plugins-symmetric {
  display: flex;
  gap: 20px;
  justify-content: space-between;
}

.plugin-column {
  flex: 1;
  min-width: 0;
}

.plugin-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.plugin-list li {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid var(--border-color);
}

.plugin-list li:last-child {
  border-bottom: none;
}

.plugin-link {
  font-size: 1rem;
  color: var(--sidebar-active);
  text-decoration: none;
  cursor: pointer;
}

.plugin-link:hover {
  text-decoration: underline;
}

.plugin-id {
  font-size: 12px;
  color: var(--text-color);
  opacity: 0.7;
}

.loading {
  color: var(--text-color);
  font-size: 14px;
  padding: 10px 0;
  text-align: center;
}

.no-plugins {
  color: var(--text-color);
  font-size: 14px;
  padding: 10px 0;
  font-style: italic;
  text-align: center;
}

/* 插件错误信息样式 */
.plugin-error {
  padding: 20px;
  background-color: rgba(244, 67, 54, 0.1);
  border: 1px solid rgba(244, 67, 54, 0.3);
  border-radius: 5px;
  margin: 10px 0;
}

.error-message {
  color: #f44336;
  font-size: 14px;
  text-align: center;
  line-height: 1.5;
}
</style>