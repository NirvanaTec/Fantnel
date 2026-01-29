import axios from "axios";
import router from "../router";

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

// 切换账号
async function switchAccount(id) {
    return axios.get(`/api/gameaccount/switch?id=${id}`).then(res => res.data);
}

// 获取当前选择账号信息
async function getGameAccount() {
    return axios.get(`/api/gameaccount/current`).then(res => res.data);
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
async function getServerInfo(id) {
    return axios.get(`/api/gameserver/getlaunch?id=${id}`).then(res => res.data);
}

// 添加游戏名称
async function addServerRole(id, name) {
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
    return axios.get(getCaptcha4399Url());
}

// 获取 4399验证码 图片URL
async function getCaptcha4399Url() {
    return "/api/gameaccount/captcha4399";
}

// 验证 4399验证码
async function verifyCaptcha4399(text) {
    return axios.post("/api/gameaccount/captcha4399/verify", text).then(res => res.data);
}

// 获取 4399 验证码内容
async function getCaptcha4399Content() {
    return axios.get(`/api/gameaccount/captcha4399/content`).then(res => res.data);
}

// 获取代理服务器信息
async function getServerInfo() {
    return axios.get(`/api/server/get`).then(res => res.data);
}

// 关闭代理服务器
async function closeServer(id) {
    return axios.get(`/api/server/close?id=${id}`).then(res => res.data);
}

// 获取游戏皮肤列表
async function getGameSkinList(offset = 0, pageSize = 10) {
    return axios.get(`/api/gameskin/get?offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
}

// 获取游戏皮肤详情
async function getGameSkinDetail(id) {
    return axios.get(`/api/gameskin/detail?id=${id}`).then(res => res.data);
}

// 获取版本
async function getVersion() {
    try {
        const res = await axios.get(`/api/version`, {timeout: 2000});
        // 检查 data.id 是否存在
        if (!res.data.data || !res.data.data.id) {
            return {
                "code": -1,
                "data": {
                    "version": "-1",
                    "id": -1,
                    "mode": "win64"
                },
                "msg": "获取失败"
            };
        }
        return res.data;
    } catch (error) {
        return {
            "code": -1,
            "data": {
                "version": "-1",
                "id": -1,
                "mode": "win64"
            },
            "msg": "获取失败"
        };
    }
}

// 获取版本ID
async function getVersionId() {
    return getVersion().then(res => res.data.id);
}

// 是否版本安全
async function isVersionSafe(id, throwError = true) {
    // 如果小于，提示可能该版本可能不包含当前内容，然后返回 Home 页
    return getVersionId().then(id1 => {
        if (id1 < id) {
            if (throwError) {
                router.push("/version");
            }
        }
        return id1 >= id;
    });
}

// 设置游戏皮肤
async function setGameSkin(id) {
    return axios.get(`/api/gameskin/set?id=${id}`).then(res => res.data);
}

// 获取皮肤 - 根据名称
async function getGameSkinListByName(name, offset = 0, pageSize = 10) {
    return axios.get(`/api/gameskin/get?name=${name}&offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
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
    getServerInfo,
    addServerRole,
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
    getServerInfo,
    closeServer,
    getVersion,
    getVersionId,
    isVersionSafe,
    getCaptcha4399Content,
    getCaptcha4399Url,
    switchAccount,
    getGameAccount,
    getGameSkinList,
    getGameSkinDetail,
    setGameSkin,
    getGameSkinListByName,
}