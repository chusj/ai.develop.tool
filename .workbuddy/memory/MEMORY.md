# MultiAgentDevelop — 项目长期笔记

## 项目性质

从开源框架 **Blog.Core（anjoy8）** 复制改造而来，命名空间/程序集统一改成 `MultiAgentDevelop.*`（原 `BlogCore.*` / `Blog.*` 的痕迹仍在文件名和注释里，例如 `BlogCore.Data.json` 目录、AOP 类名 `BlogCacheAOP`/`BlogTranAOP`/`BlogLogAOP`、`MultiAgentDevelop.Model.xml`）。

## 技术栈

- .NET 8（`net8.0`），ASP.NET Core，Autofac 容器
- ORM：SqlSugar 5.1.4.158（CodeFirst + DbFirst 都启用），DbType 用数字配置（0=MySql, 1=SqlServer, 2=Sqlite, 3=Oracle, 5=DM, 6=Kdbndp）
- 前端：`MultiAgentDevelop.Vue3`（Vite + vue-router），dev server 由 `vite.config.ts` 把 `/api` 代理到 9291
- 另有 `MultiAgentDevelop.Gateway`（Ocelot + Nacos）、`EventBus`（RabbitMQ/Kafka/RedisMQ）、`Serilog*`、`Tasks`（Quartz）
- **数据库当前配置直指 SeeHealth 那套 Oracle**：`appsettings.json` → Main / Log 连接串都是 `192.168.1.119:1521/health`，`User Id=C##ZHOUSHAN_TIJIAN`

## 关键机制（改代码前必看）

1. **建表靠反射**：`Common/Seed/DBSeed.cs` 遍历 `MultiAgentDevelop.Model.dll`，只对 **namespace 精确 == `MultiAgentDevelop.Model.Models`** 且**不带 `[MultiTenant]`** 的类执行 `CodeFirst.InitTables`。基类（命名空间 `MultiAgentDevelop.Model` 或 `...Models.RootTkey`）不建表。**只建不删，删实体后旧表要手工 DROP。**
2. **Service/Repository 自动注册**：`Extensions/ServiceExtensions/AutofacModuleRegister.cs` 加载 `MultiAgentDevelop.Services.dll` / `Repository.dll` 并 `RegisterAssemblyTypes().AsImplementedInterfaces()`。因此 **新增/删除 Service 类无需改注册代码**，但这两个 dll 不存在时启动直接抛异常（代码里硬检查）。
3. **软删除/租户全局过滤器**：`Common/DB/RepositorySetting.cs`，只对 `BaseEntity` 子类生效（`IsSubclassOf(typeof(BaseEntity))`）。`Role`/`Modules`/`Permission` 等直接继承 `RootEntityTkey` 的实体**不走全局过滤**，靠各自 Service 手工处理。
4. **JWT 鉴权**：`Extensions/Authorizations/`，`Permissions.IsUseIds4` / `IsUseAuthing` 开关，默认走 JWT。权限点校验在 `PermissionHandler` + `Api/Filter/GlobalAuthorizeFilter.cs`。
5. **UI 托管**：`wwwroot/ui/` 由 `ui.zip` 启动时解压生成，与 `MultiAgentDevelop.Vue3` 是两套独立前端（详见 2026-10-13 日志）。

## 约定

- 交付文档写 Markdown 放项目根目录，回复里给核心结论摘要（褚哥的既定习惯）。
- 改数据库相关代码前，先确认 `appsettings.json` 的 Main 连接指向哪个库 —— 目前是 SeeHealth 的生产 Oracle。

## 相关文档

- `Model实体关联关系与清理清单.md`（项目根目录）— 实体关联图 + 引用矩阵 + 删除影响链
