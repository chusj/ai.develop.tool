# MultiAgentDevelop

MultiAgentDevelop 是一个基于 .NET 8 的多项目后端框架初始化仓库，当前保留了一套较完整的框架示例 API，同时新建了后期主站点。

## 当前定位

- `MultiAgentDevelop.Api`：框架自带 API 站点，仅作为参考、示例和能力验证入口。
- `MultiAgentDevelop.MainSite`：后期主站点，当前是 .NET 8 Razor Pages 基础项目，是后续业务建设的主要入口。
- `MultiAgentDevelop.sln`：统一解决方案，包含主站、示例 API、网关、框架类库、任务、日志、事件总线和测试项目。

## 技术栈概览

- 运行时：.NET 8，统一配置见 `Directory.Build.props`。
- Web：ASP.NET Core、Razor Pages、Controllers、Swagger。
- 依赖注入与 AOP：Autofac、Castle DynamicProxy。
- 数据访问：SqlSugar、MongoDB Driver，支持多数据库配置示例。
- 缓存与队列：MemoryCache、Redis、RabbitMQ、Kafka、InitQ。
- 日志与观测：Serilog、Seq、Elasticsearch Sink、MiniProfiler、SkyAPM 示例。
- 网关与注册发现：Ocelot、Consul、Nacos。
- 测试：xUnit、Moq、Microsoft.NET.Test.Sdk。

## 目录说明

- `MultiAgentDevelop.MainSite/`：未来主站点，当前保留默认 Razor Pages 结构，并已接入框架扩展项目引用与 Oracle 配置。
- `MultiAgentDevelop.Api/`：框架示例 API，包含控制器、认证、Swagger、中间件、种子数据配置等。
- `MultiAgentDevelop.Common/`：通用工具、缓存、配置、数据库基础设施、日志辅助、扩展方法。
- `MultiAgentDevelop.Model/`：实体模型、DTO、枚举、分页和响应模型。
- `MultiAgentDevelop.Repository/`：仓储、UnitOfWork、Mongo 仓储基础实现。
- `MultiAgentDevelop.IServices/`：服务接口层。
- `MultiAgentDevelop.Services/`：业务服务实现层。
- `MultiAgentDevelop.Extensions/`：应用启动扩展、认证授权、Swagger、日志、缓存、DB、消息、任务等装配代码。
- `MultiAgentDevelop.Tasks/`：Quartz 和 HostedService 任务。
- `MultiAgentDevelop.EventBus/`：RabbitMQ/Kafka 事件总线抽象与实现。
- `MultiAgentDevelop.Gateway/`：Ocelot 网关示例。
- `Ocelot.Provider.Nacos/`：Ocelot 与 Nacos 集成扩展。
- `MultiAgentDevelop.Tests/`：单元测试与集成测试样例。

## 快速开始

```powershell
dotnet restore .\MultiAgentDevelop.sln
dotnet build .\MultiAgentDevelop.sln
dotnet run --project .\MultiAgentDevelop.MainSite\MultiAgentDevelop.MainSite.csproj
```

如需查看框架能力示例，可运行：

```powershell
dotnet run --project .\MultiAgentDevelop.Api\MultiAgentDevelop.Api.csproj
```

## 开发注意事项

- 新业务优先落在 `MultiAgentDevelop.MainSite`，不要把示例 API 当作最终主站。
- 从示例 API 迁移框架能力时，优先复用 `Extensions` 中已有的 `Add*Setup` 和 `Use*Middle` 扩展方法。
- API 示例配置中包含数据库、JWT、Redis、RabbitMQ、Nacos 等占位配置，正式环境必须替换密钥、连接串和服务名。
- 当前 `MainSite` 已引用 `MultiAgentDevelop.Extensions`，但启动流程仍是 Razor Pages 基础模板；接入服务、仓储、认证、日志等能力时，需要继续补充 `Program.cs` 装配。
- 变更公共模型、仓储或服务接口时，同步检查 `MultiAgentDevelop.Tests` 中的相关测试。

## 推荐验证命令

```powershell
dotnet build .\MultiAgentDevelop.sln
dotnet test .\MultiAgentDevelop.Tests\MultiAgentDevelop.Tests.csproj
```
