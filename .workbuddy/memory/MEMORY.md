# MultiAgentDevelop — 项目长期笔记

## 项目性质

从开源框架 **Blog.Core（anjoy8）** 复制改造，命名空间统一为 `MultiAgentDevelop.*`。残留痕迹：`BlogCore.Data.json` 目录、AOP 类名 `BlogCacheAOP`/`BlogTranAOP`/`BlogLogAOP`。

## 技术栈

- .NET 8 + ASP.NET Core + Autofac；SqlSugar 5.1.4.158（CodeFirst + DbFirst，DbType 用数字：0=MySql,1=SqlServer,2=Sqlite,3=Oracle,5=DM,6=Kdbndp）
- 前端 `MultiAgentDevelop.Vue3`（Vite），dev server 把 `/api` 代理到 9291；另有独立的 `wwwroot/ui`（由 `ui.zip` 启动时解压，与 Vue3 无关）
- 其余：`Gateway`（Ocelot+Nacos）、`EventBus`（RabbitMQ/Kafka/RedisMQ）、`Serilog*`、`Tasks`（Quartz）
- **当前数据库指向 SeeHealth 的 Oracle**：`appsettings.json` 的 Main/Log 都是 `192.168.1.119:1521/health`，`User Id=C##ZHOUSHAN_TIJIAN`

## 关键机制（改代码前必看）

1. **建表靠反射**：`Common/Seed/DBSeed.cs` 只对 **namespace 精确 == `MultiAgentDevelop.Model.Models`** 且**不带 `[MultiTenant]`** 的类执行 `CodeFirst.InitTables`。基类不建表。**只建不删，删实体后旧表要手工 DROP。** 放错命名空间的 DTO 会被误建表。
2. **Service/Repository 自动注册**：`AutofacModuleRegister.cs` 加载 `Services.dll` / `Repository.dll` 并 `RegisterAssemblyTypes().AsImplementedInterfaces()`，增删 Service 无需改注册代码；但这两个 dll 缺失时启动直接抛异常。
3. **软删除/租户过滤器**：`Common/DB/RepositorySetting.cs` 只对 `BaseEntity` 子类生效。`Role`/`Modules`/`Permission` 直接继承 `RootEntityTkey`，**不走全局过滤**，靠各自 Service 手工处理。
4. **JWT 鉴权**：`Extensions/Authorizations/`，权限点校验在 `PermissionHandler` + `Api/Filter/GlobalAuthorizeFilter.cs`。
5. **主键全部非自增**：`RootEntityTkey<Tkey>.Id` 只标 `[SugarColumn(IsPrimaryKey=true)]`，**全项目无 `IsIdentity`** → Id 由代码显式赋值。种子 tsv 删条目后 **Id 留空洞安全，无需重排**。
6. **种子 tsv 三份互相关联**：`Permission.Mid→Modules.Id`、`Permission.Pid→Permission.Id`、`RoleModulePermission.ModuleId→Modules.Id`、`.PermissionId→Permission.Id`。`Permission.tsv` 是 JSONC（含 `//{ }` 注释块）。

## 项目定位（2026-10-13 全部确认并执行完毕）

不做博客/话题/留言/密码库、不做微信、不做多租户业务演示 —— 均已整块删除。**Quartz 定时任务保留**（基础设施，`JobBase` 依赖 `TasksQz`/`TasksLog`）；`SysTenant` 保留（`SysUserInfo.TenantId` OneToOne 真导航）。

`Models/` 现存 12 个实体：SysUserInfo / UserRole / Role / RoleModulePermission / Permission / Modules / SysTenant / Department / OperateLog / AccessTrendLog / TasksQz / TasksLog，另有 `RootTkey/` 下 8 个基类。

## 约定

- 交付文档写 Markdown 放项目根目录，回复给核心结论摘要。
- 改数据库相关代码前，先确认 `appsettings.json` 的 Main 连接指向哪个库（目前是 SeeHealth 生产 Oracle）。
- **扫描实体引用必须同时搜 `Xxx` 和 `IXxxServices`**：只搜实体名会漏掉构造函数注入（`\bBlogArticle\b` 匹配不到 `IBlogArticleServices`），A/B 档各踩一次。
- **`Controllers/Tenant/TenantManagerController` 管的是 `SysTenant` 核心表**，同目录另 3 个 `TenantBy*Controller` 才是多租户演示 —— 勿按目录整体删。
- **破坏性脚本纪律**：结果变量勿与循环变量重名（曾把 `Modules.tsv` 写成 24 字节字符串）；备份必须在写入前完成且"目标不存在才备份"；改格式敏感文件用文本级精确切除 + "零删除重建 == 原文"自检（详见 2026-10-13 日志）。

## 相关文档

- `Model实体关联关系与清理清单.md`（项目根目录）— 关联图 + 引用矩阵 + 删除影响链（18 项）+ A/B/C 档执行记录 + 种子菜单数据清理记录。
