<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-6">游戏管理</h1>
    
    <!-- 游戏列表 -->
    <div v-if="games.length > 0" class="bg-gray-800 rounded-lg overflow-hidden">
      <table class="w-full">
        <thead>
          <tr class="border-b border-gray-700">
            <th class="py-3 px-4 text-left">游戏名称</th>
            <th class="py-3 px-4 text-left">服务器ID</th>
            <th class="py-3 px-4 text-left">角色名称</th>
            <th class="py-3 px-4 text-left">用户ID</th>
            <th class="py-3 px-4 text-left">游戏版本</th>
            <th class="py-3 px-4 text-left">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="game in games" :key="game.game_id" class="border-b border-gray-700 hover:bg-gray-750">
            <td class="py-3 px-4">{{ game.game_name }}</td>
            <td class="py-3 px-4">{{ game.game_id }}</td>
            <td class="py-3 px-4">{{ game.role_name }}</td>
            <td class="py-3 px-4">{{ game.user_id }}</td>
            <td class="py-3 px-4">{{ game.game_version }}</td>
            <td class="py-3 px-4">
              <button @click="closeGame(game.id)" class="text-red-400 hover:text-red-300">
                关闭游戏
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-else class="bg-gray-800 rounded-lg p-6 text-center">
      <p class="text-gray-400">暂无启动的游戏</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { getLaunchedGames, closeGame as apiCloseGame } from '../services/api'

const games = ref([])

// 初始化
onMounted(() => {
  loadGames()
})

// 加载游戏列表
const loadGames = async () => {
  try {
    const response = await getLaunchedGames()
    if (response.data.code === 1) {
      games.value = response.data.data
    }
  } catch (error) {
    console.error('Failed to load games:', error)
  }
}

// 关闭游戏
const closeGame = async (id) => {
  try {
    const response = await apiCloseGame(id)
    if (response.data.code === 1) {
      await loadGames()
    }
  } catch (error) {
    console.error('Failed to close game:', error)
  }
}
</script>

<style scoped>
.bg-gray-750 {
  background-color: rgba(59, 130, 246, 0.1);
}
</style>
