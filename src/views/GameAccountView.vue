<template>
  <div class="p-6">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold">游戏账号管理</h1>
    </div>

    <!-- 游戏账号列表 -->
    <div class="bg-gray-800 rounded-lg overflow-hidden">
      <table class="w-full">
        <thead>
          <tr class="border-b border-gray-700">
            <th class="py-3 px-4 text-left">名称</th>
            <th class="py-3 px-4 text-left">账号</th>
            <th class="py-3 px-4 text-left">类型</th>
            <th class="py-3 px-4 text-left">userId</th>
            <th class="py-3 px-4 text-left">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="account in gameAccounts" :key="account.id" class="border-b border-gray-700 hover:bg-gray-750">
            <td class="py-3 px-4">{{ account.name }}</td>
            <td class="py-3 px-4">{{ account.account }}</td>
            <td class="py-3 px-4">{{ account.type }}</td>
            <td class="py-3 px-4">{{ account.userId || '-' }}</td>
            <td class="py-3 px-4">
              <div class="flex gap-2">
                <button @click="loginAccount(account)" class="text-green-400 hover:text-green-300">
                  登录
                </button>
                <button @click="editAccount(account)" class="text-blue-400 hover:text-blue-300">
                  编辑
                </button>
                <button @click="deleteAccount(account.id)" class="text-red-400 hover:text-red-300">
                  删除
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <button @click="showAddModal = true"
      class="fixed bottom-6 right-6 bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors shadow-lg z-40">
      添加账号
    </button>

    <!-- 添加账号模态框 -->
    <div v-if="showAddModal" class="fixed inset-0 bg-gray-900 bg-opacity-70 flex items-center justify-center z-50">
      <div class="bg-gray-800 rounded-lg p-6 w-96">
        <h2 class="text-xl font-bold mb-4">{{ isEditing ? '编辑账号' : '添加账号' }}</h2>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-gray-400 mb-1">名称</label>
            <input type="text" v-model="accountForm.name"
              class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">账号</label>
            <input type="text" v-model="accountForm.account"
              class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">密码</label>
            <input type="text" v-model="accountForm.password"
              class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">类型</label>
            <select v-model="accountForm.type"
              class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
              <option value="4399">4399</option>
              <option value="4399com">4399Com</option>
              <option value="163Email">163Email</option>
              <option value="cookie">Cookie/Auth</option>
            </select>
          </div>
          <div class="flex gap-2">
            <button @click="saveAccount"
              class="flex-1 bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
              保存
            </button>
            <button @click="showAddModal = false"
              class="flex-1 bg-gray-700 hover:bg-gray-600 rounded px-4 py-2 transition-colors">
              取消
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 登录账号模态框 -->
    <div v-if="showLoginModal" class="fixed inset-0 bg-gray-900 bg-opacity-70 flex items-center justify-center z-50">
      <div class="bg-gray-800 rounded-lg p-6 w-96">
        <h2 class="text-xl font-bold mb-4">登录账号执行</h2>
        <div class="space-y-4">
          <div v-if="loginStatus === 'captcha'">
            <div class="flex justify-center mb-4">
              <img :src="captchaImage" alt="验证码" class="border border-gray-600 rounded cursor-pointer"
                style="width: 100px; height: 50px;" @click="refreshCaptcha">
            </div>
            <div>
              <label class="block text-sm text-gray-400 mb-1">验证码</label>
              <input type="text" v-model="captchaCode" placeholder="输入验证码"
                class="w-full bg-gray-700 border border-gray-600 rounded px-3 py-2 text-white focus:outline-none focus:ring-1 focus:ring-blue-400">
            </div>
          </div>
          <div v-else-if="loginStatus === 'success'" class="text-center">
            <div class="text-green-400 text-lg mb-2">登录成功！</div>
          </div>
          <div v-else-if="loginStatus === 'error'" class="text-center">
            <div class="text-red-400 text-lg mb-2">登录失败</div>
            <div class="text-gray-300">{{ errorMessage }}</div>
          </div>
          <div v-else-if="loginStatus === 'loading'" class="text-center">
            <div class="text-gray-300">登录中...</div>
          </div>
          <div class="flex gap-2">
            <button v-if="loginStatus === 'captcha'" @click="submitCaptcha"
              class="flex-1 bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
              提交验证码
            </button>
            <button v-else @click="showLoginModal = false"
              class="flex-1 bg-blue-500 hover:bg-blue-600 rounded px-4 py-2 transition-colors">
              确定
            </button>
            <button @click="showLoginModal = false"
              class="flex-1 bg-gray-700 hover:bg-gray-600 rounded px-4 py-2 transition-colors">
              取消
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import {
  deleteGameAccount,
  getCaptcha4399,
  getCaptcha4399Content,
  getGameAccounts,
  saveGameAccount,
  selectGameAccount,
  updateGameAccount,
  verifyCaptcha4399
} from '../services/api'

const gameAccounts = ref([])
const showAddModal = ref(false)
const showLoginModal = ref(false)
const isEditing = ref(false)
const accountForm = ref({
  name: '',
  account: '',
  type: '4399',
  password: ''
})

// 登录相关状态
const currentAccount = ref(null)
const loginStatus = ref('')
const captchaImage = ref('')
const captchaCode = ref('')
const errorMessage = ref('')

// 初始化
onMounted(() => {
  loadGameAccounts()
})

// 加载游戏账号
const loadGameAccounts = async () => {
  try {
    const response = await getGameAccounts()
    if (response.data.code === 1) {
      gameAccounts.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load game accounts:', error)
  }
}

// 删除账号
const deleteAccount = async (id) => {
  try {
    const response = await deleteGameAccount(id)
    if (response.data.code === 1) {
      await loadGameAccounts()
    }
  } catch (error) {
    console.error('Failed to delete account:', error)
  }
}

// 编辑账号
const editAccount = (account) => {
  isEditing.value = true
  accountForm.value = { ...account }
  showAddModal.value = true
}

// 保存账号
const saveAccount = async () => {
  try {
    let response
    if (isEditing.value) {
      response = await updateGameAccount(accountForm.value)
    } else {
      response = await saveGameAccount(accountForm.value)
    }
    if (response.data.code === 1) {
      await loadGameAccounts()
      showAddModal.value = false
      resetForm()
    }
  } catch (error) {
    console.error('Failed to save account:', error)
  }
}

// 重置表单
const resetForm = () => {
  accountForm.value = {
    name: '',
    account: '',
    type: '4399',
    password: ''
  }
  isEditing.value = false
}

// 登录账号执行
const loginAccount = async (account) => {
  currentAccount.value = account
  showLoginModal.value = true

  if (account.type === '4399' || account.type === '4399com') {
    try {
      loginStatus.value = 'loading'
      // 获取验证码图片（直接返回blob）
      const captchaResponse = await getCaptcha4399()
      // 将blob转换为Data URL
      captchaImage.value = URL.createObjectURL(captchaResponse.data)
      loginStatus.value = 'captcha'

      // 自动获取验证码内容
      const autoCaptchaResponse = await getCaptcha4399Content()
      if (autoCaptchaResponse.data.code === 1) {
        captchaCode.value = autoCaptchaResponse.data.data
      }
    } catch (error) {
      console.error('Failed to get captcha:', error)
      loginStatus.value = 'error'
      errorMessage.value = '获取验证码失败'
    }
  } else {
    // 其他类型账号直接设置为优先账号（系统会自动处理登录）
    try {
      loginStatus.value = 'loading'
      // 将账号设置为优先账号
      const response = await selectGameAccount(account.id)
      if (response.data.code === 1) {
        loginStatus.value = 'success'
        // 更新账号信息
        await loadGameAccounts()
      } else {
        loginStatus.value = 'error'
        errorMessage.value = response.data.msg || '登录失败'
      }
    } catch (error) {
      console.error('Failed to login:', error)
      loginStatus.value = 'error'
      errorMessage.value = '登录出错'
    }
  }
}

// 自动获取验证码内容
const autoGetCaptcha = async () => {
  try {
    const response = await getCaptcha4399Content()
    if (response.data.code === 1) {
      captchaCode.value = response.data.data
    } else {
      alert('自动获取验证码失败')
    }
  } catch (error) {
    console.error('Failed to auto get captcha:', error)
    alert('自动获取验证码失败')
  }
}

// 刷新验证码
const refreshCaptcha = async () => {
  try {
    const captchaResponse = await getCaptcha4399()
    captchaImage.value = URL.createObjectURL(captchaResponse.data)

    // 清空之前的验证码内容
    captchaCode.value = ''

    // 重新自动获取验证码内容
    autoGetCaptcha()
  } catch (error) {
    console.error('Failed to refresh captcha:', error)
    alert('刷新验证码失败')
  }
}

// 提交验证码
const submitCaptcha = async () => {
  try {
    loginStatus.value = 'loading'
    // 对于4399类型账号，使用verifyCaptcha4399接口
    const response = await verifyCaptcha4399({
      id: currentAccount.value.id,
      captcha: captchaCode.value
    })
    if (response.data.code === 1) {
      // 将账号设置为优先账号
      const response1 = await selectGameAccount(currentAccount.value.id)
      if (response1.data.code === 1) {
        loginStatus.value = 'success'
        // 更新账号信息
        await loadGameAccounts()
      } else {
        loginStatus.value = 'error'
        errorMessage.value = response1.data.msg || '登录失败'
      }
    } else {
      loginStatus.value = 'error'
      errorMessage.value = response.data.msg || '验证失败'
    }
  } catch (error) {
    console.error('Failed to submit captcha:', error)
    loginStatus.value = 'error'
    errorMessage.value = '验证出错'
  }
}
</script>

<style scoped></style>
