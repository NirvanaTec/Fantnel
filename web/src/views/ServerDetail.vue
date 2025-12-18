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
        <form @submit.prevent="launchGame1">
          <div class="form-group">
            <label>账号:</label>
            <select v-model="selectedAccount" @change="selectAccount1">
              <option v-for="account in accounts" :key="account.id" :value="account.id">{{ account.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>游戏名称:</label>
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
            <!-- <button type="submit" class="launch-game-btn">启动游戏</button> -->
            <button type="button" @click="launchProxy" class="launch-proxy-btn">启动代理</button>
            <button type="button" @click="showLaunchModal = false" class="cancel-btn">取消</button>
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
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { selectServer, getLaunchInfo, addLaunchGame, selectAccount, launchGame } from '../utils/Tools'
import Alert from '../components/Alert.vue'

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
const newGameName = ref('')
const selectedGame = ref('')

// 提示框
const showNotice = ref(false)
const noticeText = ref("")
const noticeLocation = ref("")

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
  getLaunchInfo1();
})

function selectAccount1(event) {
  selectAccount(event.target.value).then(data => {
    if (data.code === 1) {
      getLaunchInfo1();
    } else {
      noticeText.value = data.msg;
      showNotice.value = true;
    }
  });
}

function getLaunchInfo1() {
  getLaunchInfo(serverId).then(data => {
    accounts.value = data.data.accounts
    games.value = data.data.games

    // 账号默认选中
    for (let account of accounts.value) {
      if (account.userId !== null) {
        selectedAccount.value = account.id;
        break
      }
    }

    // 名称默认选中
    if (games.value && games.value.length > 0) {
      selectedGame.value = games.value[0].name
    } else {
      showAddGameInput.value = true
    }
  })
}

function switchImage(img) {
  mainImage.value = img
}

function launchGame1() {
  if (!selectedGame.value) {
    noticeText.value = '请选择游戏名称';
    showNotice.value = true;
    return;
  }
  // alert(`启动游戏: ${selectedGame.value}，账号: ${selectedAccount.value}`)
  showLaunchModal.value = false
  noticeText.value = "正在启动游戏中，请稍后.....";
  launchGame(serverId, selectedGame.value, "game").then(data => {
    // 启动游戏完成，更多信息请查看控制台
    noticeText.value = data.msg;
  }).catch(err => {
    noticeText.value = err.message;
  });
  showNotice.value = true;
}

function launchProxy() {
  if (!selectedGame.value) {
    noticeText.value = '请选择游戏名称';
    showNotice.value = true;
    return;
  }
  // alert(`启动游戏: ${selectedGame.value}，账号: ${selectedAccount.value}`)
  showLaunchModal.value = false
  noticeText.value = "正在启动代理中，请稍后.....";
  launchGame(serverId, selectedGame.value, "proxy").then(data => {
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
    addLaunchGame(serverId, newGameName.value).then(data => {
      if (data.code === 1) {
        getLaunchInfo1();
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
</style>