<template>
  <div class="skin-detail">
    <div class="skin-header">
      <div class="skin-title">
        <h1>{{ skin.name }}</h1>
        <p>皮肤ID: {{ skin.entity_id }}</p>
      </div>
      <button class="download-btn" @click="downloadSkin">使用皮肤</button>
    </div>

    <div class="skin-images">
      <img :src="skin.title_image_url" alt="皮肤介绍图" class="main-image">
    </div>

    <div class="skin-meta">
      <div class="meta-item">
        <span class="label">皮肤作者:</span>
        <span class="value">{{ skin.developer_name }}</span>
      </div>
      <div class="meta-item">
        <span class="label">发布时间:</span>
        <span class="value">{{ skin.publish_time }}</span>
      </div>
      <div class="meta-item">
        <span class="label">下载量:</span>
        <span class="value">{{ skin.download_num }}</span>
      </div>
      <div class="meta-item">
        <span class="label">点赞数:</span>
        <span class="value">{{ skin.like_num }}</span>
      </div>
    </div>

    <div class="skin-description">
      <h2>皮肤介绍</h2>
      <p>{{ skin.brief_summary }}</p>
    </div>

    <!-- 使用Alert组件 -->
    <Alert :show="showNotice" :message="noticeText" :title="alertType === 'confirm' ? '确认使用' : '提示'"
      :location="noticeLocation" :showCancel="alertType === 'confirm'" @ok="handleNoticeOk" @close="showNotice = false"
      @cancel="handleNoticeCancel" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { getGameSkinDetail, setGameSkin } from '../../../utils/Tools'

const route = useRoute()
const skinId = route.params.id

const skin = ref({})

// 提示框
const showNotice = ref(false)
const noticeText = ref("")
const noticeLocation = ref("")
const alertType = ref("") // 用于区分提示类型：confirm

onMounted(() => {
  fetchSkinDetail()
})

// 获取皮肤详情
const fetchSkinDetail = async () => {
  try {
    const res = await getGameSkinDetail(skinId)
    if (res.code === 1) {
      skin.value = res.data;
    } else {
      noticeText.value = res.msg || "获取皮肤详情失败"
      showNotice.value = true
      noticeLocation.value = "/game-accounts";
    }
  } catch (error) {
    noticeText.value = "获取皮肤详情失败"
    showNotice.value = true
    noticeLocation.value = "/game-accounts";
  }
}

// 下设置载皮肤
const downloadSkin = () => {
  // 显示确认使用提示
  alertType.value = 'confirm'
  noticeText.value = `确认要使用 "${skin.value.name}" 皮肤吗？`
  showNotice.value = true
}

function handleNoticeOk() {
  if (alertType.value === 'confirm') {
    // 开始设置皮肤
    alertType.value = ''
    noticeText.value = "正在设置皮肤中，请稍后....."
    setGameSkin(skinId).then(res => {
      noticeText.value = res.msg || "皮肤设置失败"
    })
  } else {
    // 其他类型的提示，直接关闭
    showNotice.value = false
    if (noticeLocation.value && noticeLocation.value !== '') {
      location.href = noticeLocation.value
    }
  }
}

// 处理取消操作
function handleNoticeCancel() {
  showNotice.value = false
  alertType.value = ''
}
</script>

<style scoped>
.skin-detail {
  padding: 20px;
}

.skin-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.skin-title h1 {
  margin: 0;
  font-size: 24px;
}

.skin-title p {
  margin: 5px 0 0 0;
  color: var(--text-color);
  font-size: 14px;
}

.download-btn {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  transition: background-color 0.3s;
}

.download-btn:hover {
  background-color: #45a049;
}

.skin-images {
  margin-bottom: 20px;
}

.main-image {
  width: 30%;
  max-width: 584px;
  height: auto;
  object-fit: cover;
  border-radius: 5px;
  margin-bottom: 10px;
}

.skin-meta {
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

.skin-description {
  margin-top: 20px;
}

.skin-description h2 {
  margin: 0 0 10px 0;
  font-size: 20px;
  color: var(--text-color);
}

.skin-description p {
  line-height: 1.6;
  color: var(--text-color);
  font-size: 16px;
}
</style>