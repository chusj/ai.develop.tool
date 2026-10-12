# AGENT.md

本文件给 AI 编程工具使用，用于快速理解当前仓库边界、结构和开发约定。

## 项目状态

- 仓库根目录：`f:\trae-proj\MultiAgentDevelop`
- 解决方案：`MultiAgentDevelop.sln`
- 统一目标框架：`Directory.Build.props` 设置 `TargetFramework=net8.0`、`ImplicitUsings=enable`
- `README.md` 面向人类维护者；本文档面向 AI 工具。
- 重要边界：当前整体采用前后端分离方式；`MultiAgentDevelop.Vue3` 是 Vue3 前端项目，后端仍通过 API 项目提供接口能力。

## 入口项目

- `MultiAgentDevelop.Vue3`
  - 前端主项目，基于 Vue3、Vite、TypeScript。
  - 使用 npm 管理依赖，`package-lock.json` 需要随依赖变更一起维护。
  - 负责页面、路由、状态管理和接口调用；后端业务能力通过 API 接口提供。
  - 前端构建产物和本地依赖不入库，见根目录 `.gitignore` 的 Vue / Vite 规则。
- `MultiAgentDevelop.Api`
  - 后端 API 项目，也是当前前后端分离架构下的接口服务参考入口。
  - 启动代码展示 Autofac、配置加载、SqlSugar、种子数据、Serilog、Swagger、JWT/Ids4/Authing、Redis、RabbitMQ、Kafka、Nacos、AOP、中间件、SignalR、MiniProfiler 等能力的完整接入方式。
  - 新增或迁移接口时优先保持现有 API 契约、统一响应结构和序列化行为一致。
- `MultiAgentDevelop.Gateway`
  - Ocelot 网关示例，包含 Swagger 聚合、自定义认证、JWT 中间件、Consul/Ocelot/Polly 相关依赖。

## 项目分层

- `MultiAgentDevelop.Model`：实体、DTO、枚举、分页、统一响应模型。
- `MultiAgentDevelop.Common`：公共工具、缓存、配置读取、数据库基础设施、HTTP、加密、日志、SignalR Hub、扩展方法。
- `MultiAgentDevelop.Repository`：SqlSugar 仓储基类、UnitOfWork、Mongo 仓储。
- `MultiAgentDevelop.IServices`：服务接口层。
- `MultiAgentDevelop.Services`：业务服务实现，依赖接口层和仓储层。
- `MultiAgentDevelop.Extensions`：应用启动和中间件扩展，是示例 API 能力装配的主要位置。
- `MultiAgentDevelop.Tasks`：Quartz 任务和 HostedService。
- `MultiAgentDevelop.EventBus`：RabbitMQ/Kafka 事件总线抽象与实现。
- `MultiAgentDevelop.Serilog`、`MultiAgentDevelop.Serilog.Es`：日志扩展与格式化。
- `Ocelot.Provider.Nacos`：Ocelot 与 Nacos 集成。
- `MultiAgentDevelop.FrameWork`：T4/代码生成相关工程。
- `MultiAgentDevelop.Tests`：xUnit/Moq 测试项目，目前引用示例 API。
- `MultiAgentDevelop.Vue3`：Vue3 + Vite + TypeScript 前端项目，包含页面、布局、路由、Pinia 状态、Axios 接口封装等。

## 依赖方向

- 典型后端业务链路：`Api -> Extensions -> Services -> IServices -> Repository -> Common/Model`
- 典型前端调用链路：`Vue3 views/components -> api/* -> utils/axiosInstance -> 后端 API`
- `Repository` 引用 `Common` 和 `Model`。
- `Services` 引用 `IServices` 和 `Repository`。
- `Extensions` 引用 `EventBus`、`Serilog`、`Services`、`Tasks`。
- `Common` 引用 `Model`、`Serilog.Es`、`Ocelot.Provider.Nacos`。
- `Gateway` 引用 `Extensions` 和 `Model`。
- `Vue3` 不直接引用后端项目，通过 HTTP API 与后端交互。

## 开发原则

- 优先保护用户已有改动；不要还原未明确要求还原的文件。
- 前端页面、路由、状态和接口调用优先落到 `MultiAgentDevelop.Vue3`。
- 后端接口、服务、仓储、模型和基础设施仍按现有 .NET 分层维护。
- 参考框架能力时从 `MultiAgentDevelop.Api\Program.cs` 和 `MultiAgentDevelop.Extensions\ServiceExtensions\*.cs` 查找接入方式。
- 对外 API 契约迁移时保持路由、返回结构、序列化行为一致，特别注意 Newtonsoft.Json 配置：
  - `ReferenceLoopHandling.Ignore`
  - `DefaultContractResolver`
  - `DateFormatString = "yyyy-MM-dd HH:mm:ss"`
  - `DateTimeZoneHandling.Local`
  - `StringEnumConverter`
  - Int64 转字符串的 `NumberConverter`
- 示例 API 中的 `appsettings.json` 含示例连接串、JWT Secret、第三方服务占位信息；正式代码不得新增真实密钥。
- 涉及数据库、缓存、消息队列、注册中心时，先检查配置开关和 `Enabled` 字段。
- 需要接入框架能力时，优先复用已有扩展方法，不要重复散落注册代码。

## 常用命令

```powershell
dotnet restore .\MultiAgentDevelop.sln
dotnet build .\MultiAgentDevelop.sln
dotnet run --project .\MultiAgentDevelop.Api\MultiAgentDevelop.Api.csproj
dotnet test .\MultiAgentDevelop.Tests\MultiAgentDevelop.Tests.csproj
npm install --prefix .\MultiAgentDevelop.Vue3
npm run dev --prefix .\MultiAgentDevelop.Vue3
npm run build --prefix .\MultiAgentDevelop.Vue3
```

## 文件编辑约定

- 文档和代码默认保持现有风格。
- `.csproj` 变更后优先执行 `dotnet build .\MultiAgentDevelop.sln`。
- `package.json` 或前端依赖变更后，同步维护 `MultiAgentDevelop.Vue3\package-lock.json`，并优先执行 `npm run build --prefix .\MultiAgentDevelop.Vue3`。
- 修改服务/仓储/模型时，检查对应接口、实现、依赖注入和测试。
- 修改前端接口调用时，检查 `src\api\*.ts`、`src\utils\axiosInstance.ts` 和相关页面是否保持一致。
- 修改示例 API 时，确认用户是否希望影响框架示例；否则只把它当参考。

## 当前已知事项

- `AGENT.md` 和 `README.md` 原本为空，本次初始化用于建立项目认知。
- `MultiAgentDevelop.Api.csproj` 没有显式 `TargetFramework`，由 `Directory.Build.props` 继承 `net8.0`。
- `MultiAgentDevelop.MainSite` 已删除，不再作为主站入口维护。
- `MultiAgentDevelop.Vue3` 已导入为前端项目，后续整体按前后端分离方式演进。
- `MultiAgentDevelop.Api` 的 Debug 输出和部分库项目输出路径仍偏向旧示例结构，后续后端接口集成时需重新评估输出路径。
