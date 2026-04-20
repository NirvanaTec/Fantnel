# Fantnel

Fantnel 是一个基于 .NET 10.0 开发的 完全脱离盒子 项目。本项目基于 [Codexus.OpenSDK](https://github.com/denetease/OpenSDK.NEL) 二次开发。

## 🚀 核心特性

### 技术优势
1. **可安装插件提醒** - 提醒用户该服务器支持 "某些" 插件，避免用户手动寻找插件
2. **AI 文档完善** - [完整的 AI 文档，助力 Fantnel UI 生成](web/FantnelAi/)
3. **BmclApi 支持** - 借助 BmclApi 来为 Linux/MacOS 库资源修复功能
4. **启动参数解析** - 参考 FaithXL 启动器实现启动参数解析与生成
5. **插件兼容性** - 支持 Nirvana.Development 插件，与 Codexus.Development 同时运行, 且无冲突
6. **CodexusApi 加速 [涅槃云]** - 借助 Codexus API 加速插件商城功能，提供更快的插件下载速度和更新检查
7. **缓存优化** - 自动获取 服务器/租凭服/皮肤 的消息和主图片，缓存到本地，避免重复请求

### 用户体验
1. **插件自动更新** - 已安装插件自动更新
2. **用户聊天室** - 代理模式内实现用户聊天功能
3. **跨平台支持** - 支持 Windows、Linux、macOS 多平台运行
4. **自定义主题** - 支持个性化主题设置，用户可根据喜好选择不同界面风格
5. **Nac4399** - 支持自动从 Nac4399 获取随机游戏账号，无需手动获取。

### IRC 跨服聊天 [ 代理模式 ]
- 内置聊天室
- 支持 跨服务器 / 跨系统 聊天
- 支持与 FaithX / SpaceCL 等 用户聊天
- 支持 1.20.6 / 1.20.1 / 1.12.2 / 1.8.x 等更多版本
- 支持 随时开启，随时关闭
- 基于 Nirvana.Development 开发

### 插件系统 [ 代理模式 ]
- 支持 Nirvana.Development 插件，与 Codexus.Development 同时运行, 且无冲突
- 支持插件商城功能，提供更快的插件下载速度和更新检查
- 可手动关闭自动更新插件功能，为开发者提供更多控制权

### 启动游戏 - 跨系统 [ 独家技术 ]
- 支持 Windows、Linux、macOS 多平台启动游戏
- 支持自定义游戏参数，JVM参数、最大内存等
- 借助 BmclApi 修复 Netease Client 导致的 Lwjgl 、Natives 库资源问题。
---
- 独家技术 是 截至 2026.4.20 目前为止, 无第二位支持 Linux、macOS 上运行 Netease Client 的项目


## 🛠️ 技术架构

### 开发环境
- **框架**: .NET 10.0
- **语言**: C# 12
- **Web 框架**: ASP.NET Core Web API
- **日志系统**: Serilog

### 多平台支持
项目支持以下平台的构建和运行：
- **Windows**: x64, ARM64
- **Linux**: x64, ARM64
- **MacOS**: x64, ARM64

## 🤝 参与贡献

欢迎社区成员为 Fantnel 项目做出贡献！

### 参与方式
1. **问题反馈** - 在 GitHub 提交 Issue
2. **功能建议** - 提出新功能或改进建议
3. **代码贡献** - 提交 Pull Request
4. **文档完善** - 改进项目文档

### 开发规范
- 遵循 C# 编码规范
- 保持代码注释清晰
- 编写单元测试
- 遵循 Git 提交规范

## 📄 开源协议
```
本程序是自由软件，你可以重新发布或修改它，但必须：
1. 保留原始版权声明
2. 采用相同许可证分发  
3. 提供完整的源代码

详细条款请查阅 LICENSE 文件。
```

## 👥 开发团队

**涅槃科技**
- 开发者: 风横
- QQ: 3547694806
- 邮箱: fengheng1111@126.com

**涅槃频道**
- QQ 频道: 请前往官网来加入涅槃频道

## 🌐 官方资源

- **官方网站**: https://npyyds.top/
- **Gitee 仓库**: https://gitee.com/newNP/fantnel/
- **GitHub 仓库**: https://github.com/NirvanaTec/fantnel/

---

*最终解释权归于 涅槃科技 所有*