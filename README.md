# MultiAgentDevelop

MultiAgentDevelop 是一个基于 .NET 8 后端框架和 Vue3 前端项目的前后端分离初始化仓库，当前保留了一套较完整的后端 API 能力，并导入了 Vue3 前端主项目。

## 当前定位

- `MultiAgentDevelop.Vue3`：Vue3 前端主项目，负责页面、路由、状态管理和接口调用。
- `MultiAgentDevelop.Api`：后端 API 项目，提供接口服务，并保留框架能力接入示例。
- `MultiAgentDevelop.sln`：后端统一解决方案，包含 API、网关、框架类库、任务、日志、事件总线和测试项目。

## 技术栈概览

- 运行时：.NET 8，统一配置见 `Directory.Build.props`。
- 后端 Web：ASP.NET Core、Controllers、Swagger。
- 前端 Web：Vue3、Vite、TypeScript、Vue Router、Pinia、Element Plus、Axios。
- 依赖注入与 AOP：Autofac、Castle DynamicProxy。
- 数据访问：SqlSugar、MongoDB Driver，支持多数据库配置示例。
- 缓存与队列：MemoryCache、Redis、RabbitMQ、Kafka、InitQ。
- 日志与观测：Serilog、Seq、Elasticsearch Sink、MiniProfiler、SkyAPM 示例。
- 网关与注册发现：Ocelot、Consul、Nacos。
- 测试：xUnit、Moq、Microsoft.NET.Test.Sdk。

## 目录说明

- `MultiAgentDevelop.Vue3/`：Vue3 + Vite + TypeScript 前端项目，包含页面、布局、路由、Pinia 状态、Axios 接口封装等。
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
dotnet run --project .\MultiAgentDevelop.Api\MultiAgentDevelop.Api.csproj
```

启动前端开发服务：

```powershell
npm install --prefix .\MultiAgentDevelop.Vue3
npm run dev --prefix .\MultiAgentDevelop.Vue3
```

构建前端：

```powershell
npm run build --prefix .\MultiAgentDevelop.Vue3
```

## 开发注意事项

- 前端页面、路由、状态和接口调用优先落在 `MultiAgentDevelop.Vue3`。
- 后端接口、服务、仓储、模型和基础设施仍按现有 .NET 分层维护。
- 从示例 API 迁移框架能力时，优先复用 `Extensions` 中已有的 `Add*Setup` 和 `Use*Middle` 扩展方法。
- API 示例配置中包含数据库、JWT、Redis、RabbitMQ、Nacos 等占位配置，正式环境必须替换密钥、连接串和服务名。
- 前后端对接时优先保持 API 路由、返回结构和序列化行为稳定，避免破坏现有页面调用。
- 前端依赖变更后需要同步提交 `MultiAgentDevelop.Vue3/package-lock.json`。
- 变更公共模型、仓储或服务接口时，同步检查 `MultiAgentDevelop.Tests` 中的相关测试。

## 推荐验证命令

```powershell
dotnet build .\MultiAgentDevelop.sln
dotnet test .\MultiAgentDevelop.Tests\MultiAgentDevelop.Tests.csproj
npm run build --prefix .\MultiAgentDevelop.Vue3
```
