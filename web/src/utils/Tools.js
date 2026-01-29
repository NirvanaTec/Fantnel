import axios from "axios";
import router from "../router";

// Home 页
export async function getHome_Info() {
    return axios("/api/home").then(res => res.data);
}

// Account 页
export async function getAccounts() {
    return axios("/api/gameaccount/get").then(res => res.data);
}

// 添加账号
export async function addAccount(formData) {
    return axios.post("/api/gameaccount/save", formData).then(res => res.data);
}

// 登录账号
export async function selectAccount(id) {
    return axios.get(`/api/gameaccount/select?id=${id}`).then(res => res.data);
}

// 切换账号
export async function switchAccount(id) {
    return axios.get(`/api/gameaccount/switch?id=${id}`).then(res => res.data);
}

// 获取当前选择账号信息
export async function getGameAccount() {
    return axios.get(`/api/gameaccount/current`).then(res => res.data);
}

// 删除账号
export async function deleteAccount(id) {
    return axios.get(`/api/gameaccount/delete?id=${id}`).then(res => res.data);
}

// 更新账号
export async function updateAccount(formData) {
    return axios.post("/api/gameaccount/update", formData).then(res => res.data);
}

// 获取服务器列表
export async function getServerList(offset = 0, pageSize = 10) {
    return axios.get(`/api/gameserver/get?offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
}

// 获取服务器详情
export async function selectServer(id) {
    return axios.get(`/api/gameserver/id?id=${id}`).then(res => res.data);
}

// 获取服务器 账号/角色
export async function getServerInfo(id) {
    return axios.get(`/api/gameserver/getlaunch?id=${id}`).then(res => res.data);
}

// 添加游戏名称
export async function addServerRole(id, name) {
    return axios.post("/api/gameserver/createname", {id, name}).then(res => res.data);
}

// 启动游戏代理
export async function launchProxy(id, name, mode = "net") {
    return axios.get(`/api/gameserver/launch?id=${id}&name=${name}&mode=${mode}`).then(res => res.data);
}

// 启动游戏白端
export async function launchGame(id, name, mode = "net") {
    return axios.get(`/api/gamelaunch/launch?id=${id}&name=${name}&mode=${mode}`).then(res => res.data);
}

// 获取插件列表
export async function getPlugins() {
    return axios.get("/api/plugins/get").then(res => res.data);
}

// 切换插件状态
export async function togglePluginStatus(id) {
    return axios.get(`/api/plugins/toggle?id=${id}`).then(res => res.data);
}

// 删除插件
export async function deletePlugin(id) {
    return axios.get(`/api/plugins/delete?id=${id}`).then(res => res.data);
}

// 获取所有插件 - 插件商城
export async function getPluginList() {
    return axios.get("/api/pluginstore/get").then(res => res.data);
}

// 获取插件详情
export async function getPluginDetail(id) {
    return axios.get(`/api/pluginstore/detail?id=${id}`).then(res => res.data);
}

// 安装插件
export async function installPlugin(id) {
    return axios.get(`/api/pluginstore/install?id=${id}`).then(res => res.data);
}

// 获取主题名
export async function getThemeName() {
    return axios.get(`/api/theme`).then(res => res.data);
}

// 切换主题
export async function setThemeName(name) {
    return axios.get(`/api/theme/set?name=${name}`).then(res => res.data);
}

// 获取 4399验证码 图片
export async function getCaptcha4399() {
    return axios.get(getCaptcha4399Url());
}

// 获取 4399验证码 图片URL
export async function getCaptcha4399Url() {
    return "/api/gameaccount/captcha4399";
}

// 验证 4399验证码
export async function verifyCaptcha4399(text) {
    return axios.post("/api/gameaccount/captcha4399/verify", text).then(res => res.data);
}

// 获取 4399 验证码内容
export async function getCaptcha4399Content() {
    return axios.get(`/api/gameaccount/captcha4399/content`).then(res => res.data);
}

// 获取白端游戏信息
export async function getGameLaunchInfo() {
    return axios.get(`/api/gamelaunch/get`).then(res => res.data);
}

// 关闭白端游戏
export async function closeGameLaunch(id) {
    return axios.get(`/api/gamelaunch/close?id=${id}`).then(res => res.data);
}

// 获取代理服务器信息
export async function getProxyServerInfo() {
    return axios.get(`/api/server/get`).then(res => res.data);
}

// 关闭代理服务器
export async function closeProxyServer(id) {
    return axios.get(`/api/server/close?id=${id}`).then(res => res.data);
}

// 获取游戏皮肤列表
export async function getGameSkinList(offset = 0, pageSize = 10) {
    return axios.get(`/api/gameskin/get?offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
}

// 获取游戏皮肤详情
export async function getGameSkinDetail(id) {
    return axios.get(`/api/gameskin/detail?id=${id}`).then(res => res.data);
}

// 获取版本
export async function getVersion() {
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
export async function getVersionId() {
    return getVersion().then(res => res.data.id);
}

// 是否版本安全
export async function isVersionSafe(id, throwError = true) {
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
export async function setGameSkin(id) {
    return axios.get(`/api/gameskin/set?id=${id}`).then(res => res.data);
}

// 获取皮肤 - 根据名称
export async function getGameSkinListByName(name, offset = 0, pageSize = 10) {
    return axios.get(`/api/gameskin/get?name=${name}&offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
}

// 获取租赁服列表
export async function getRentalServerList(offset = 0, pageSize = 10) {
    return axios.get(`/api/gamerental/get?offset=${offset}&pageSize=${pageSize}`).then(res => res.data);
}

// 获取租赁服详情
export async function getRentalServerDetail(id) {
    return axios.get(`/api/gamerental/id?id=${id}`).then(res => res.data);
}

// 租赁服排序
export async function sortRentalServer() {
    return axios.get(`/api/gamerental/sort`);
}

// 添加租赁服角色
export async function addRentalRole(id, name) {
    return axios.post(`/api/gamerental/createname`, {id, name}).then(res => res.data);
}

// 获取租赁服 账号/角色
export async function getRentalInfo(id) {
    return axios.get(`/api/gamerental/getlaunch?id=${id}`).then(res => res.data);
}