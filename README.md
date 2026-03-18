# Fantnel WebUI - 涅槃深灰蓝

## 项目概述

**Fantnel WebUI** 是一个基于 Vue 3 的现代化 Web 应用程序，用于管理和监控 Fantnel 项目的运行状态。该项目采用了深色主题设计，提供了丰富的游戏服务器管理、插件管理、皮肤管理等功能。

### 项目信息

- **项目名称**: Fantnel WebUI
- **项目 ID**: nirvana.dark.slate.blue
- **技术栈**: Vue 3 + Vite + Vue Router + Tailwind CSS 4

---

### 核心技术栈

- **前端框架**: Vue 3
- **构建工具**: Vite 7
- **路由管理**: Vue Router 5
- **HTTP 请求**: Axios 1
- **UI 框架**: Tailwind CSS 4
- **图标库**: FontAwesome 7
- **工具库**: VueUse Head 2

### 项目结构

```
FantnelAi1/
├── public/                 # 静态资源目录
│   ├── favicon.ico        # 网站图标
│   ├── vite.svg           # Vite 图标
│   └── random.name.json   # 随机名称数据
├── src/                   # 源代码目录
│   ├── assets/           # 资源文件
│   ├── components/       # 可复用组件
│   │   ├── HelloWorld.vue
│   │   ├── StatusModal.vue      # 状态提示模态框
│   │   └── StatusModal1.vue     # 确认对话框模态框
│   ├── router/           # 路由配置
│   │   └── index.js
│   ├── services/         # API 服务层
│   │   └── api.js
│   ├── views/            # 页面视图组件
│   │   ├── HomeView.vue              # 主页
│   │   ├── GameAccountView.vue       # 游戏账号管理
│   │   ├── GameManagementView.vue    # 游戏管理
│   │   ├── LogView.vue               # 日志查看
│   │   ├── MySkinView.vue            # 我的皮肤
│   │   ├── NetworkServerView.vue     # 网络服务器列表
│   │   ├── NetworkServerDetailView.vue  # 网络服务器详情
│   │   ├── NirvanaUserView.vue       # 涅槃用户信息
│   │   ├── PluginManagementView.vue  # 插件管理
│   │   ├── PluginStoreView.vue       # 插件商城
│   │   ├── PluginStoreDetailView.vue # 插件详情
│   │   ├── ProxyManagementView.vue   # 代理管理
│   │   ├── RentalServerView.vue      # 租赁服务器列表
│   │   ├── RentalServerDetailView.vue # 租赁服务器详情
│   │   ├── SkinDetailView.vue        # 皮肤详情
│   │   └── SystemSettingsView.vue    # 系统设置
│   ├── App.vue           # 根组件
│   ├── main.js           # 应用入口
│   ├── style.css         # 全局样式
│   └── tools.js          # 工具函数
├── index.html            # HTML 入口文件
├── package.json          # 项目依赖配置
├── vite.config.js        # Vite 配置文件
└── tailwind.config.js    # Tailwind CSS 配置文件
```

---

## 功能模块

### 1. 主页 (HomeView)

**功能描述**:
- 显示系统信息（游戏版本、CRC Salt 等）
- 支持拖拽上传主题文件（.fant.json 格式）
- 展示广告信息区域

**API 接口**:
- `GET /api/home` - 获取主页信息
- `POST /api/theme/switch` - 应用主题

---

### 2. 游戏账号管理 (GameAccountView)

**功能描述**:
- 管理游戏账号列表（添加、编辑、删除）
- 支持多种账号类型：4399、4399Com、163Email、Cookie/Auth
- 验证码自动识别功能（针对 4399 账号）
- 账号登录和切换

**API 接口**:
- `GET /api/gameaccount/get` - 获取账号列表
- `POST /api/gameaccount/save` - 保存账号
- `POST /api/gameaccount/update` - 更新账号
- `GET /api/gameaccount/delete?id=` - 删除账号
- `GET /api/gameaccount/switch?id=` - 切换账号
- `GET /api/gameaccount/captcha4399` - 获取验证码图片
- `GET /api/gameaccount/captcha4399/content` - 自动获取验证码内容
- `POST /api/gameaccount/captcha4399/verify` - 验证验证码

---

### 3. 服务器管理

#### 3.1 网络服务器 (NetworkServerView)

**功能描述**:
- 展示网络服务器列表（分页显示）
- 显示服务器在线人数、简介等信息
- 支持跳转到服务器详情页

**API 接口**:
- `GET /api/gameserver/get?offset=&pageSize=` - 获取服务器列表
- `GET /api/gameserver/id?id=` - 获取服务器详情
- `GET /api/gameserver/getlaunch?id=` - 获取启动信息
- `POST /api/gameserver/createname` - 创建角色

#### 3.2 租赁服务器 (RentalServerView)

**功能描述**:
- 展示租赁服务器列表（分页显示）
- 支持跳转到服务器详情页

**API 接口**:
- `GET /api/gamerental/get?offset=&pageSize=` - 获取租赁服列表
- `GET /api/gamerental/id?id=` - 获取租赁服详情
- `GET /api/gamerental/getlaunch?id=` - 获取启动信息
- `POST /api/gamerental/createname` - 创建角色

---

### 4. 皮肤管理 (MySkinView)

**功能描述**:
- 展示游戏皮肤列表（支持搜索）
- 分页显示皮肤信息
- 显示作者、下载量等信息
- 支持跳转到皮肤详情页

**API 接口**:
- `GET /api/gameskin/get?offset=&pageSize=&name=` - 获取皮肤列表
- `GET /api/gameskin/detail?id=` - 获取皮肤详情
- `GET /api/gameskin/set?id=` - 设置皮肤

---

### 5. 插件管理

#### 5.1 插件管理 (PluginManagementView)

**功能描述**:
- 管理已安装的插件
- 开启/关闭插件
- 删除插件

**API 接口**:
- `GET /api/plugins/get` - 获取已安装插件
- `GET /api/plugins/toggle?id=` - 切换插件状态
- `GET /api/plugins/delete?id=` - 删除插件

#### 5.2 插件商城 (PluginStoreView)

**功能描述**:
- 浏览可安装的插件
- 查看插件详情
- 安装插件及依赖

**API 接口**:
- `GET /api/pluginstore/get` - 获取插件商城列表
- `GET /api/pluginstore/detail?id=` - 获取插件详情
- `GET /api/pluginstore/install?id=` - 安装插件
- `GET /api/plugins/dependence?id=&version=` - 获取插件依赖

---

### 6. 代理管理 (ProxyManagementView)

**功能描述**:
- 显示当前运行的代理列表
- 展示代理详细信息（服务器名称、版本、角色名、端口等）
- 复制代理信息到剪贴板
- 关闭代理

**API 接口**:
- `GET /api/server/get` - 获取代理列表
- `GET /api/server/close?id=` - 关闭代理

---

### 7. 游戏管理 (GameManagementView)

**功能描述**:
- 显示已启动的游戏列表
- 展示游戏信息（名称、服务器 ID、角色名、用户 ID、版本）
- 关闭游戏

**API 接口**:
- `GET /api/gamelaunch/get` - 获取已启动游戏
- `GET /api/gamelaunch/close?id=` - 关闭游戏

---

### 8. 系统设置 (SystemSettingsView)

**功能描述**:
- **主动登录设置**:
  - 开启主动登录游戏账号
  - 开启主动登录 Cookie 游戏账号
  - 开启主动登录 163Email 游戏账号
- **聊天室设置**:
  - 启用游戏聊天室
- **启动配置**:
  - 启动游戏最大内存设置
  - JVM 参数配置
  - 游戏参数配置
- **其它配置**:
  - 自动更新插件设置

**API 接口**:
- `GET /api/nirvana/get` - 获取系统配置
- `GET /api/nirvana/set?mode=&value=` - 设置系统配置

---

### 9. 涅槃账号系统

**功能描述**:
- 涅槃账号登录/登出
- 查看账号信息
- 账号状态管理

**API 接口**:
- `GET /api/nirvana/login?account=&password=` - 登录账号
- `GET /api/nirvana/account/get` - 获取账号信息
- `GET /api/nirvana/logout` - 登出账号

---

### 10. 日志查看 (LogView)

**功能描述**:
- 查看系统运行日志

**API 接口**:
- `GET /api/logs` - 获取日志信息

---

## 路由配置

| 路径 | 名称 | 组件 | 说明 |
|------|------|------|------|
| `/` | home | HomeView | 主页 |
| `/game-account` | gameAccount | GameAccountView | 游戏账号管理 |
| `/network-server` | networkServer | NetworkServerView | 网络服务器列表 |
| `/network-server/:id` | networkServerDetail | NetworkServerDetailView | 网络服务器详情 |
| `/rental-server` | rentalServer | RentalServerView | 租赁服务器列表 |
| `/rental-server/:id` | rentalServerDetail | RentalServerDetailView | 租赁服务器详情 |
| `/my-skin` | mySkin | MySkinView | 我的皮肤 |
| `/my-skin/:id` | skinDetail | SkinDetailView | 皮肤详情 |
| `/plugin-management` | pluginManagement | PluginManagementView | 插件管理 |
| `/plugin-store` | pluginStore | PluginStoreView | 插件商城 |
| `/plugin-store/:id` | pluginStoreDetail | PluginStoreDetailView | 插件详情 |
| `/proxy-management` | proxyManagement | ProxyManagementView | 代理管理 |
| `/game-management` | gameManagement | GameManagementView | 游戏管理 |
| `/system-settings` | systemSettings | SystemSettingsView | 系统设置 |
| `/nirvana-user` | nirvanaUser | NirvanaUserView | 涅槃用户信息 |
| `/logs` | logs | LogView | 日志信息 |

---

## 开发指南

### 环境要求

- Node.js 18+ 
- npm 或 yarn

### 安装依赖

```bash
npm install
```

### 启动开发服务器

```bash
npm run dev
```

开发服务器将在 `http://localhost:5173` 启动，并代理 API 请求到 `http://localhost:13521`

### 构建生产版本

```bash
npm run build
```

### 预览生产构建

```bash
npm run preview
```

---

## 设计特点

### UI/UX 设计

- **深色主题**: 采用深灰色系（gray-900、gray-800 等）作为主色调
- **蓝色强调色**: 使用蓝色（blue-400、blue-500）作为高亮和操作按钮颜色
- **响应式布局**: 使用 Tailwind CSS 的响应式工具类
- **平滑过渡**: 页面切换和交互效果使用过渡动画
- **模态框**: 提供状态提示和确认对话框

### 技术特性

- **组件化开发**: 所有页面都是独立的 Vue 组件
- **组合式 API**: 使用 `<script setup>` 语法
- **响应式数据**: 使用 Vue 3 的 ref 和 reactive
- **路由懒加载**: 使用动态导入实现路由懒加载
- **图标系统**: 使用 FontAwesome 图标库

---

## API 服务层

所有 API 请求都通过 `src/services/api.js` 统一管理，使用 Axios 创建实例，配置了：

- 基础请求头：`Content-Type: application/json`
- 开发环境代理：`/api` -> `http://localhost:13521`

---

## 主题系统

支持自定义主题，主题文件格式为 `.fant.json`，通过拖拽或点击上传到主页即可应用。

---

## 联系方式

- **官网**: https://npyyds.top/
- **Gitee**: https://gitee.com/newNP/fantnel/tree/nirvana.dark.slate.blue/
- **GitHub**: https://github.com/NirvanaTec/fantnel/tree/nirvana.dark.slate.blue/
- **开发者 QQ**: 3547694806
- **开发者邮箱**: fengheng1111@126.com

---

## 商用声明

您可以使用 Fantnel WebUI 作为您的商用项目，但我们不提供任何形式的担保或承诺，包括但不限于质量、性能和安全性。详细条款请查阅 LICENSE 文件。

## 贡献指南

如果您想为 Fantnel 做出贡献，或者有任何问题想要咨询，欢迎加入我们的官方 QQ 频道，或者在 GitHub 上提交 issue 或 pull request。

让我们一起携手，共同推动公益技术的发展！
