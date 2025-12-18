import axios from "axios";

// Home 页
async function getHome_Info() {
    return axios("/api/home").then(res => res.data);
}

// Account 页
async function getAccounts() {
    return axios("/api/gameaccount/get").then(res => res.data);
}

// 添加账号
async function addAccount(formData) {
    return axios.post("/api/gameaccount/save", formData).then(res => res.data);
}

// 登录账号
async function selectAccount(id) {
    return axios.get(`/api/gameaccount/select?id=${id}`).then(res => res.data);
}

// 删除账号
async function deleteAccount(id) {
    return axios.get(`/api/gameaccount/delete?id=${id}`).then(res => res.data);
}

// 更新账号
async function updateAccount(formData) {
    return axios.post("/api/gameaccount/update", formData).then(res => res.data);
}

// 获取服务器列表
async function getServerList(offset = 0, pageSize = 10) {
    return axios.get(`/api/gameserver/get?offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
}

// 获取服务器详情
async function selectServer(id) {
    return axios.get(`/api/gameserver/id?id=${id}`).then(res => res.data);
}

// 获取启动信息
async function getLaunchInfo(id) {
    return axios.get(`/api/gameserver/getlaunch?id=${id}`).then(res => res.data);
}

// 添加游戏名称
async function addLaunchGame(id, name) {
    return axios.post("/api/gameserver/createname", {id, name}).then(res => res.data);
}

// 启动游戏
async function launchGame(id, name, mode) {
    return axios.get(`/api/gameserver/launch?id=${id}&name=${name}&mode=${mode}`).then(res => res.data);
}

// 获取插件列表
async function getPlugins() {
    return axios.get("/api/plugins/get").then(res => res.data);
}

// 切换插件状态
async function togglePluginStatus(id) {
    return axios.get(`/api/plugins/toggle?id=${id}`).then(res => res.data);
}

// 删除插件
async function deletePlugin(id) {
    return axios.get(`/api/plugins/delete?id=${id}`).then(res => res.data);
}

// 获取所有插件 - 插件商城
async function getPluginList() {
    return axios.get("/api/pluginstore/get").then(res => res.data);
}

// 获取插件详情
async function getPluginDetail(id) {
    return axios.get(`/api/pluginstore/detail?id=${id}`).then(res => res.data);
}

// 安装插件
async function installPlugin(id) {
    return axios.get(`/api/pluginstore/install?id=${id}`).then(res => res.data);
}

// 获取主题名
async function getThemeName() {
    return axios.get(`/api/theme`).then(res => res.data);
}

// 切换主题
async function setThemeName(name) {
    return axios.get(`/api/theme/set?name=${name}`).then(res => res.data);
}

// 获取 4399验证码 图片
async function getCaptcha4399() {
    return axios.get(`/api/gameaccount/captcha4399`);
}

// 验证 4399验证码
async function verifyCaptcha4399(text) {
    return axios.post("/api/gameaccount/captcha4399/verify", text).then(res => res.data);
}

export {
    getHome_Info,
    getAccounts,
    addAccount,
    selectAccount,
    deleteAccount,
    updateAccount,
    getServerList,
    selectServer,
    getLaunchInfo,
    addLaunchGame,
    launchGame,
    getPlugins,
    togglePluginStatus,
    deletePlugin,
    getPluginList,
    getPluginDetail,
    installPlugin,
    getThemeName,
    setThemeName,
    getCaptcha4399,
    verifyCaptcha4399,
}
