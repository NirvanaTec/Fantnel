import axios from 'axios'


const apiClient = axios.create({
  headers: {
    'Content-Type': 'application/json'
  }
})

// 主页相关
export const getHomeInfo = () => apiClient.get('/api/home')

// 游戏账号相关
export const getGameAccounts = () => apiClient.get('/api/gameaccount/get')
export const getAvailableGameAccounts = () => apiClient.get('/api/gameaccount/available')
export const switchGameAccount = (id) => apiClient.get(`/api/gameaccount/switch?id=${id}`)
export const selectGameAccount = (id) => apiClient.get(`/api/gameaccount/select?id=${id}`)
export const getCurrentGameAccount = () => apiClient.get('/api/gameaccount/current')
export const deleteGameAccount = (id) => apiClient.get(`/api/gameaccount/delete?id=${id}`)
export const saveGameAccount = (data) => apiClient.post('/api/gameaccount/save', data)
export const updateGameAccount = (data) => apiClient.post('/api/gameaccount/update', data)

export const getCaptcha4399 = () => apiClient.get('/api/gameaccount/captcha4399', { responseType: 'blob' })
export const getCaptcha4399Content = () => apiClient.get('/api/gameaccount/captcha4399/content')
export const verifyCaptcha4399 = (data) => apiClient.post('/api/gameaccount/captcha4399/verify', data)

// 网络服务器相关
export const getNetworkServers = (offset, pageSize) => apiClient.get(`/api/gameserver/get?offset=${offset}&pageSize=${pageSize}`)
export const getNetworkServerDetail = (id) => apiClient.get(`/api/gameserver/id?id=${id}`)
export const getNetworkServerLaunchInfo = (id) => apiClient.get(`/api/gameserver/getlaunch?id=${id}`)
export const createNetworkServerRole = (data) => apiClient.post('/api/gameserver/createname', data)

// 租赁服务器相关
export const getRentalServers = (offset, pageSize) => apiClient.get(`/api/gamerental/get?offset=${offset}&pageSize=${pageSize}`)
export const getRentalServerDetail = (id) => apiClient.get(`/api/gamerental/id?id=${id}`)
export const getRentalServerLaunchInfo = (id) => apiClient.get(`/api/gamerental/getlaunch?id=${id}`)
export const createRentalServerRole = (data) => apiClient.post('/api/gamerental/createname', data)

// 启动相关
export const launchProxy = (id, name, mode) => apiClient.get(`/api/gameserver/launch?id=${id}&name=${name}&mode=${mode}`)
export const launchGame = (id, name, mode) => apiClient.get(`/api/gamelaunch/launch?id=${id}&name=${name}&mode=${mode}`)

// 插件相关
export const getInstalledPlugins = () => apiClient.get('/api/plugins/get')
export const togglePluginStatus = (id) => apiClient.get(`/api/plugins/toggle?id=${id}`)
export const deletePlugin = (id) => apiClient.get(`/api/plugins/delete?id=${id}`)
export const getPluginStore = () => apiClient.get('/api/pluginstore/get')
export const getPluginDetail = (id) => apiClient.get(`/api/pluginstore/detail?id=${id}`)
export const installPlugin = (id) => apiClient.get(`/api/pluginstore/install?id=${id}`)
export const getPluginDependencies = (serverId, version) => apiClient.get(`/api/plugins/dependence?id=${serverId}&version=${version}`)

// 主题相关
export const getTheme = () => apiClient.get('/api/theme')
export const setTheme = (name) => apiClient.get(`/api/theme/set?name=${name}`)
export const setThemeSwitch = (value) => apiClient.post(`/api/theme/switch`, value)

// 游戏启动相关
export const getLaunchedGames = () => apiClient.get('/api/gamelaunch/get')
export const closeGame = (id) => apiClient.get(`/api/gamelaunch/close?id=${id}`)

// 代理相关
export const getLaunchedProxies = () => apiClient.get('/api/server/get')
export const closeProxy = (id) => apiClient.get(`/api/server/close?id=${id}`)

// 皮肤相关
export const getGameSkins = (offset, pageSize, name = '') => {
  if (name) {
    return apiClient.get(`/api/gameskin/get?name=${name}&offset=${offset}&pageSize=${pageSize}`)
  }
  return apiClient.get(`/api/gameskin/get?offset=${offset}&pageSize=${pageSize}`)
}
export const getSkinDetail = (id) => apiClient.get(`/api/gameskin/detail?id=${id}`)
export const setSkin = (id) => apiClient.get(`/api/gameskin/set?id=${id}`)

// 涅槃账号相关
export const loginNirvanaAccount = (account, password) => apiClient.get(`/api/nirvana/login?account=${account}&password=${password}`)
export const getNirvanaAccountInfo = () => apiClient.get('/api/nirvana/account/get')
export const logoutNirvanaAccount = () => apiClient.get('/api/nirvana/logout')

// 系统设置相关
export const getSystemConfig = () => apiClient.get('/api/nirvana/get')
export const setSystemConfig = (mode, value) => apiClient.get(`/api/nirvana/set?mode=${mode}&value=${value}`)

// 版本信息
export const getVersionInfo = () => apiClient.get('/api/version')

// 日志相关
export const getLogs = () => apiClient.get('/api/logs')