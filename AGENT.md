# AGENT.md

本文件给 AI 编程工具使用，用于快速理解当前仓库边界、结构和开发约定。

## 项目状态

- 仓库根目录：`f:\trae-proj\MultiAgentDevelop`
- 解决方案：`MultiAgentDevelop.sln`
- 统一目标框架：`Directory.Build.props` 设置 `TargetFramework=net8.0`、`ImplicitUsings=enable`
- `README.md` 面向人类维护者；本文档面向 AI 工具。
- 重要边界：`MultiAgentDevelop.Api` 是框架自带 API 示例站点，仅作参考和示例；`MultiAgentDevelop.MainSite` 是后期主站点。

## 入口项目

- `MultiAgentDevelop.MainSite`
  - 后续主站入口。
  - 当前是 ASP.NET Core Razor Pages 默认结构。
  - `Program.cs` 仅注册 `AddRazorPages`、静态文件、路由和授权。
  - 当前已引用 `MultiAgentDevelop.Extensions`，并配置 Oracle 主库/日志库；接入具体框架能力时仍需补充启动装配。
- `MultiAgentDevelop.Api`
  - 框架示例 API。
  - 启动代码展示 Autofac、配置加载、SqlSugar、种子数据、Serilog、Swagger、JWT/Ids4/Authing、Redis、RabbitMQ、Kafka、Nacos、AOP、中间件、SignalR、MiniProfiler 等能力的完整接入方式。
  - 不要默认把新业务继续写入此项目，除非用户明确要求维护示例 API。
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

## 依赖方向

- 典型业务链路：`Api/MainSite -> Extensions -> Services -> IServices -> Repository -> Common/Model`
- `Repository` 引用 `Common` 和 `Model`。
- `Services` 引用 `IServices` 和 `Repository`。
- `Extensions` 引用 `EventBus`、`Serilog`、`Services`、`Tasks`。
- `Common` 引用 `Model`、`Serilog.Es`、`Ocelot.Provider.Nacos`。
- `Gateway` 引用 `Extensions` 和 `Model`。
- `MainSite` 当前引用 `Extensions`，但不要假设其已具备示例 API 的完整启动能力。

## 开发原则

- 优先保护用户已有改动；不要还原未明确要求还原的文件。
- 新主站相关工作优先落到 `MultiAgentDevelop.MainSite`，除非用户明确指定其他项目。
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
dotnet run --project .\MultiAgentDevelop.MainSite\MultiAgentDevelop.MainSite.csproj
dotnet run --project .\MultiAgentDevelop.Api\MultiAgentDevelop.Api.csproj
dotnet test .\MultiAgentDevelop.Tests\MultiAgentDevelop.Tests.csproj
```

## 文件编辑约定

- 文档和代码默认保持现有风格。
- `.csproj` 变更后优先执行 `dotnet build .\MultiAgentDevelop.sln`。
- 修改服务/仓储/模型时，检查对应接口、实现、依赖注入和测试。
- 修改主站启动流程时，注意 `MainSite` 当前是最小 Razor Pages 模板，不要直接复制示例 API 的全部中间件；按实际需求逐项迁移。
- 修改示例 API 时，确认用户是否希望影响框架示例；否则只把它当参考。

## 当前已知事项

- `AGENT.md` 和 `README.md` 原本为空，本次初始化用于建立项目认知。
- `MultiAgentDevelop.Api.csproj` 没有显式 `TargetFramework`，由 `Directory.Build.props` 继承 `net8.0`。
- `MultiAgentDevelop.MainSite.csproj` 显式配置 `net8.0`、`Nullable=enable`、`ImplicitUsings=enable`，并参考示例 API 增加框架初始化依赖。
- `MultiAgentDevelop.Api` 的 Debug 输出和部分库项目输出路径仍偏向旧示例结构，后续主站集成时需重新评估输出路径。
