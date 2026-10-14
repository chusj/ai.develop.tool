# MultiAgentDevelop.Model 实体关联关系与清理清单

> 分析范围：`MultiAgentDevelop.Model/Models/`（含 `Models/Tenant/`、`Models/RootTkey/`）
> 分析基准：整个解决方案的源码引用（排除 `bin/`、`obj/`、`MultiAgentDevelop.Vue3/` 前端与 `MultiAgentDevelop.Tests/` 测试项目）
> 初版结论：41 个 .cs 类 = 8 个框架基类 + 1 个接口 + 32 个实体。其中 **2 个是零引用孤儿**，**22 个是框架自带的演示业务实体**。
> **当前状态（2026-10-13 全部清理完成后）**：四轮清理共删 **62 个代码文件**（A 档 7 + B 档 36 + 微信 44 + 多租户演示 11，其中 B 档含前端）；`Models/` 下剩 **12 个实体 + 8 个基类**，`Models/Tenant/` 目录已删除。剩余实体全部为必留的身份权限核心与系统表。

---

## 一、结论先行

| 分类 | 数量 | 处理建议 |
|---|---|---|
| 身份权限核心（登录/角色/菜单/部门） | 9 | **必留**，删了系统直接跑不起来 |
| 框架基类（RootTkey / BaseEntity） | 8 | **必留**，所有实体都继承它们 |
| 孤儿实体（0 引用） | 2 | **已删**（2026-10-13 A 档） |
| 演示业务（博客/话题/留言/密码库） | 6 | **已删**（2026-10-13 B 档） |
| 微信模块 | 5 | **已删**（2026-10-13 整块清理，共 44 个文件） |
| 定时任务（Quartz） | 2 | **保留**（2026-10-13 决定留作基础设施） |
| 多租户演示业务表 | 4 | **已删**（2026-10-13 C 档第二批，`SysTenant` 保留） |
| 分表演示 | 1 | **已删**（2026-10-13 A 档） |
| 测试 DTO（会被误建表） | 1 | **已删**（2026-10-13 A 档） |

**两个必须知道的坑：**

1. **`TestMuchTableResult` 会被自动建成数据库表。** 它是 `RoleModulePermissionRepository` 多表联查的结果 DTO，但命名空间恰好是 `MultiAgentDevelop.Model.Models`，被 `DBSeed` 的反射建表逻辑捕获。属于框架自带的疏漏，你的库里应该已经有这张空表。
2. **删除实体类只是停止建表，不会删表。** `DBSeed.SeedAsync` 里写死了 `IsAnyTable` 后才 `InitTables`，只增不删（源码注释明确写了"这里只支持添加表，不支持删除"）。清理完代码后，数据库里的旧表要手工 `DROP`。

---

## 二、建表机制（先看懂这个，删除才安全）

`MultiAgentDevelop.Common/Seed/DBSeed.cs` 第 89-107 行：

```csharp
var modelTypes = referencedAssemblies
   .SelectMany(a => a.DefinedTypes)
   .Select(type => type.AsType())
   .Where(x => x.IsClass && x.Namespace is "MultiAgentDevelop.Model.Models")   // 精确匹配命名空间
   .Where(s => !s.IsDefined(typeof(MultiTenantAttribute), false))              // 排除多租户实体
   .ToList();
modelTypes.ForEach(t => { if (!myContext.Db.DbMaintenance.IsAnyTable(t.Name)) myContext.Db.CodeFirst.SplitTables().InitTables(t); });
```

由此推出三条硬规则：

- **命名空间必须精确等于 `MultiAgentDevelop.Model.Models` 才会建表。**
  `Models/RootTkey/` 下的基类命名空间是 `MultiAgentDevelop.Model.Models.RootTkey`，`RootEntityTkey` 是 `MultiAgentDevelop.Model` —— 都不在其中，所以 8 个基类不会生成表。
- **带 `[MultiTenant]` 的实体走另一条路**：`TenantSeedAsync` → `TenantUtil.GetTenantEntityTypes()`，按租户分表/分库建立。
- **`RepositorySetting` 的软删除/租户过滤器**只认 `BaseEntity` 的子类（`RepositorySetting.AllEntitys`）。所以 `Role`、`Modules`、`Permission` 这些直接继承 `RootEntityTkey`、手写了 `IsDeleted` 字段的实体，**不走全局软删除过滤器**，删除逻辑是在各自 Service 里手工处理的。这点在重构实体基类时要注意。

---

## 三、实体关联关系

### 3.1 身份权限核心（必留）

```
                    ┌─────────────────────┐
                    │   SysUserInfo       │  用户（主键 Id，自建 SysUserInfoRoot）
                    │  DepartmentId ──────┼──────┐
                    │  TenantId ──────────┼───┐  │
                    └─────────┬───────────┘   │  │
                              │               │  │
              ┌───────────────┼───────────┐   │  │
              │ 1:N           │ 1:N       │   │  │
              ▼               ▼           │   │  │
      ┌───────────────┐ ┌───────────┐     │   │  │
      │   UserRole    │ │OperateLog │     │   │  │
      │  UserId ──────┤ │ UserId    │     │   │  │
      │  RoleId ──┐   │ └───────────┘     │   │  │
      └───────────┼───┘                   │   │  │
                  │                       │   │  │
                  ▼                       │   │  │
            ┌───────────┐                 │   │  │
            │   Role    │◄────────────────┘   │  │
            │ Authority │                    │  │
            │  Scope    │                    │  │
            └─────┬─────┘                    │  │
                  │ 1:N                      │  │
                  ▼                          │  │
      ┌───────────────────────┐              │  │
      │ RoleModulePermission  │  ← 三张表的核心枢纽
      │  RoleId               │              │  │
      │  ModuleId ──────┐     │              │  │
      │  PermissionId ──┼──┐  │              │  │
      └─────────────────┼──┼──┘              │  │
                        │  │                 │  │
          ┌─────────────┘  └──────────┐      │  │
          ▼                           ▼      │  │
   ┌────────────┐            ┌────────────┐  │  │
   │  Modules   │◄───────────┤ Permission │  │  │
   │ (接口/API) │  Mid       │ (路由菜单/ │  │  │
   │  ParentId  │            │  按钮)     │  │  │
   └────────────┘            │  Pid 自关联│  │  │
                             └────────────┘  │  │
                                             │  │
                    ┌────────────────────┐   │  │
                    │    Department      │◄──┼──┘
                    │  Pid (自关联树)    │   │
                    └────────────────────┘   │
                    ┌────────────────────┐   │
                    │    SysTenant       │◄──┘
                    │  (多租户)          │
                    └────────────────────┘
```

关键字段级关系（都写死在代码里，不是 EF 的导航属性推断）：

| 关系 | 载体 | 类型 | 说明 |
|---|---|---|---|
| 用户 → 部门 | `SysUserInfo.DepartmentId : long` | 弱关联，无导航属性 | 仅存 Id，配合 `Role.AuthorityScope` 做数据权限 |
| 用户 → 租户 | `SysUserInfo.TenantId` + `[Navigate(OneToOne, nameof(TenantId))]` | 真导航 | 唯一一处让 `SysTenant` 参与查询的地方 |
| 用户 ↔ 角色 | `UserRole.UserId` / `UserRole.RoleId` | 中间表 | 无导航属性，靠 Service 手工拼 |
| 角色 ↔ 菜单 ↔ 接口 | `RoleModulePermission.RoleId` / `.ModuleId` / `.PermissionId` | 三向中间表 | 权限系统的核心，`PermissionController` 读写 |
| 菜单自关联 | `Permission.Pid`（0 = 顶级） | 树形，Service 递归 | `Permission.Children` 是 `IsIgnore` 内存字段 |
| 菜单 → 接口 | `Permission.Mid` | 指向 `Modules.Id` | |
| 部门自关联 | `Department.Pid`（0 = 顶级） | 树形 | `Department.hasChildren` 是 `IsIgnore` 内存字段 |
| 模块自关联 | `Modules.ParentId` | 树形 | |

### 3.2 框架基类（必留，8 个）

| 类 | 命名空间 | 作用 |
|---|---|---|
| `RootEntityTkey<Tkey>` | `MultiAgentDevelop.Model` | 所有实体的根，只提供 `Id` |
| `SysUserInfoRoot<Tkey>` | `MultiAgentDevelop.Model` | 给用户表用的独立根（`Id` + `RIDs`），因为用户表没继承 `RootEntityTkey` |
| `ModulesRoot<Tkey>` | `MultiAgentDevelop.Model` | 提供 `Modules.ParentId` |
| `PermissionRoot<Tkey>` | `MultiAgentDevelop.Model` | 提供 `Pid` / `Mid` / `PidArr` |
| `RoleModulePermissionRoot<Tkey>` | `MultiAgentDevelop.Model` | 提供 `RoleId` / `ModuleId` / `PermissionId` |
| `UserRoleRoot<Tkey>` | `MultiAgentDevelop.Model` | 提供 `UserId` / `RoleId` |
| `DepartmentRoot<Tkey>` | `MultiAgentDevelop.Model` | 提供 `Pid` / `PidArr` |
| `TopicDetailRoot<Tkey>` | `MultiAgentDevelop.Model` | 只提供 `TopicId`，**只被 TopicDetail 用**，删 Topic 时可一并删 |
| `BaseEntity` | `...Models.RootTkey` | 带 `[SugarIndex]` 的增强基类，被 4 张多租户表继承 |
| `IDeleteFilter` | `...Models.RootTkey.Interface` | 软删除过滤接口，`BaseEntity` 实现 |

> 注意：这些基类虽然名字带 "RootTkey"，但命名空间是 `MultiAgentDevelop.Model`，**不会**被 `DBSeed` 建表。

### 3.3 演示业务（Blog.Core 自带示例）— 已于 2026-10-13 B 档删除

> 下表 9 个实体中：**`BlogArticle` ~ `PasswordLib` 6 个演示业务已删除**；`AccessTrendLog` / `OperateLog` 是**日志统计表，已保留**（`Job_AccessTrendLog_Quartz` / `Job_OperateLog_Quartz` 依赖它们）；`SplitDemo` 已随 A 档删除。
> 下表保留供追溯原关联结构。

| 实体 | 关联关系 | 引用者 |
|---|---|---|
| `BlogArticle` | `[Navigate(OneToOne, bsubmitter)]` → `SysUserInfo`；`[Navigate(OneToMany, bID)]` → `BlogArticleComment` | `BlogController`、`BlogArticleServices`、`ValuesController`、`CustomProfile`(AutoMapper)、`GuestbookViewModels`、`TestRestSharpGetDto` |
| `BlogArticleComment` | `bID` → `BlogArticle`；`UserId` → `SysUserInfo` | **只有测试项目在用**，生产代码零引用 |
| `Topic` | 内存导航 → `TopicDetail` | `TopicController`、`EventBusKafka`（Kafka 示例消费者直接用了 Topic 做消息体） |
| `TopicDetail` | `TopicId` → `Topic` | `TopicDetailController` |
| `Guestbook` | `blogId` → `BlogArticle`（`IsIgnore` 属性） | `GuestbookServices`、`TransactionController`（当测试数据用）、`Job_Blogs_Quartz` |
| `PasswordLib` | 独立，无外键 | `PasswordLibServices`、`TransactionController`、`ValuesController`、`Job_Blogs_Quartz` |
| `AccessTrendLog` | 独立 | `AccessTrendLogServices` + `Job_AccessTrendLog_Quartz`（从日志文件汇总访问人次） |
| `OperateLog` | `UserId`（`IsIgnore` 导航 → `SysUserInfo`） | `OperateLogServices` + `Job_OperateLog_Quartz`（从日志文件汇总操作记录） |
| `SplitDemo` | 独立，`[SplitTable(Day)]` 按天分表 | `SplitDemoController` |

### 3.4 微信模块（整块独立，5 张表）— 已于 2026-10-13 整块删除

原有关联结构（供追溯）：

```
WeChatConfig (主键: publicAccount)
    ├── 被 WeChatConfigServices 读取，用于公众号配置
    ├── WeChatSub.SubFromPublicAccount → WeChatConfig.publicAccount
    └── WeChatQR.QRbindCompanyID → WeChatCompany.CompanyID

WeChatCompany (主键: CompanyID)
    └── WeChatSub.CompanyID / WeChatQR.QRbindJobID

WeChatPushLog (主键: id)
    └── 独立推送日志，被 WeChatConfigServices 调用
```

无实体导航属性，全部靠字符串 ID 手工关联。前端 `Vue3/src/api/` 下没有微信 api，说明这套后端接口前端从未接入。

### 3.5 定时任务（Quartz，2 张表）— 2026-10-13 决定保留

```
TasksQz (任务定义: Cron / AssemblyName / ClassName / JobGroup)
    └── TasksLog.JobId → TasksQz.Id   (执行日志，弱关联)
```

- `TasksQz` 被 `TasksQzController`、`SchedulerCenterServer`、`ISchedulerCenter`、`JobBase` 使用
- `TasksLog` 被 `TasksQzController`、`JobBase`（所有 Job 的基类）使用
- 相关的 `Job_*` 类共 5 个：`Job_Blogs_Quartz`、`Job_AccessTrendLog_Quartz`、`Job_OperateLog_Quartz`、`Job_URL_Quartz`
- **当前开关是关闭的**：`appsettings.json` → `Middleware.QuartzNetJob.Enabled = false`

> 删 Task 表之前想清楚：`JobBase` 是基础设施，但它强依赖 `ITasksQzServices` / `ITasksLogServices`。删了就没法再跑任何定时任务。

### 3.6 多租户（原 5 张表，现仅剩 `SysTenant`）

```
SysTenant (租户定义: TenantType / ConfigId / Connection)     ← 保留
    ├── SysUserInfo.TenantId → SysTenant.Id   ← 唯一的代码级强关联
    └── TenantUtil / RepositorySetting 全局过滤器

演示业务表（BaseEntity 子类）：                                ← 已于 2026-10-13 全部删除
    BusinessTable          [ITenantEntity]          → 按租户字段隔离（TenantByIdController）
    MultiBusinessTable     [MultiTenant(Tables)]    → 按租户分表（TenantByTableController）
        └── MultiBusinessSubTable.MainId → MultiBusinessTable.Id
    SubLibraryBusinessTable [MultiTenant]           → 按租户分库（TenantByDbController）
```

原 4 张业务表 + 3 个 `Tenant*Controller`（ByDb/ById/ByTable）纯粹是演示，**已于 2026-10-13 随 C 档第二批删除**。

真正被生产链路使用的是 `SysTenant`（`SysUserInfo` 的 OneToOne 导航 + `TenantUtil` 全局过滤器），连同管理它的 `TenantManagerController`、建默认租户的 `TenantSeedData` 一起**保留**。

### 3.7 孤儿实体（零引用，已于 2026-10-13 A 档删除）

| 实体 | 表名 | 引用情况 | 判断 |
|---|---|---|---|
| `GblLogAudit` | `GblLogAudit` | **全解决方案 0 引用** | 类注释写的是"用户团队表"（明显是从别处拷来的），字段是日志审计（TraceId/Level/Logger/LogType）。框架遗留，无任何代码使用 |
| `ReportAbnoraml` | `REPORT_ABNORAML` | **全解决方案 0 引用** | 字段是 `ORGAN_ID` / `REPORT_ID` / `ABNORMAL_CODE` / `MEDICAL_EXPLAIN`（医学解释）/ `MEDICAL_LIMIT`（就医期限），是**医疗健康业务实体**，从 SeeHealth 那套库里带过来的残留 |

---

## 四、完整实体清单与引用矩阵

> 下表是**清理前的初始分析快照**（32 个实体），"分级"列为当时的处置建议。清理后的实际结果：
> - 标 `删` 的 **19 个**（#10~19、#24~32）**已全部删除**；
> - 标 `可选` 的 **5 个**（#8 `SysTenant`、#20 `OperateLog`、#21 `AccessTrendLog`、#22 `TasksQz`、#23 `TasksLog`）**全部保留**；
> - 标 `核心` 的 **7 个**（#1~7）与 **8 个基类**（#9）**全部保留**。
>
> 当前 `Models/` 下实际存在的实体 = 12 个（#1~8 + #20~23）。

| # | 实体 | 表名 | 分级 | 引用文件数 | 主要引用 |
|---|---|---|---|---|---|
| 1 | `SysUserInfo` | `sys_userinfo` | 核心 | 8 | UserController / Login、PermissionHandler、UserInfoSeedData |
| 2 | `Role` | `Role` | 核心 | 15 | RoleController、LoginController、JwtHelper、AuthorizationSetup |
| 3 | `Permission` | `Permission` | 核心 | 8 | PermissionController、GlobalAuthorizeFilter、GlobalVars |
| 4 | `Modules` | `Modules` | 核心 | 6 | ModuleController、PermissionController、RoleModulePermissionRepository |
| 5 | `RoleModulePermission` | `RoleModulePermission` | 核心 | 6 | PermissionController、RoleModulePermissionRepository |
| 6 | `UserRole` | `UserRole` | 核心 | 6 | UserController、UserRoleController、sysUserInfoServices |
| 7 | `Department` | `Department` | 核心 | 5 | DepartmentController、UserController |
| 8 | `SysTenant` | `SysTenant` | 可选 | 6 | Tenant 系列 Controller、TenantUtil、DataBaseController |
| 9 | `BaseEntity` 等 8 个基类 | — | 必留 | — | 全部实体 |
| 10 | `GblLogAudit` | `GblLogAudit` | **删** | **0** | 无 |
| 11 | `ReportAbnoraml` | `REPORT_ABNORAML` | **删** | **0** | 无 |
| 12 | `TestMuchTableResult` | `TestMuchTableResult` | **删** | 4 | RoleModulePermission 相关（可改用匿名类型/ViewModel） |
| 13 | `SplitDemo` | `SplitDemo_{yyyy}{MM}{dd}` | 删 | 3 | SplitDemoController |
| 14 | `PasswordLib` | `PasswordLib` | 删 | 5 | Job_Blogs_Quartz、TransactionController、ValuesController |
| 15 | `Guestbook` | `Guestbook` | 删 | 4 | GuestbookServices、TransactionController、Job_Blogs_Quartz |
| 16 | `BlogArticle` | `BlogArticle` | 删 | 6 | BlogController、CustomProfile、ValuesController |
| 17 | `BlogArticleComment` | `BlogArticleComment` | 删 | 0（生产） | 仅测试项目 |
| 18 | `Topic` | `Topic` | 删 | 6 | TopicController、EventBusKafka |
| 19 | `TopicDetail` | `TopicDetail` | 删 | 4 | TopicDetailController |
| 20 | `OperateLog` | `operate_log` | 可选 | 2 | Job_OperateLog_Quartz |
| 21 | `AccessTrendLog` | `AccessTrendLog` | 可选 | 2 | Job_AccessTrendLog_Quartz |
| 22 | `TasksQz` | `tasks_qz` | 可选 | 7 | TasksQzController、SchedulerCenterServer |
| 23 | `TasksLog` | `tasks_log` | 可选 | 5 | JobBase、TasksQzController |
| 24 | `WeChatCompany` | `WeChatCompany` | 删 | 3 | WeChatCompanyController |
| 25 | `WeChatConfig` | `WeChatConfig` | 删 | 3 | WeChatConfigController |
| 26 | `WeChatPushLog` | `WeChatPushLog` | 删 | 5 | WeChatPushLogController、WeChatConfigServices |
| 27 | `WeChatQR` | `WeChatQR` | 删 | 1 | 仅 WeChatConfigServices |
| 28 | `WeChatSub` | `WeChatSub` | 删 | 5 | WeChatSubController、WeChatConfigServices |
| 29 | `BusinessTable` | `BusinessTable` | 删 | 2 | TenantByIdController、BusinessDataSeedData |
| 30 | `MultiBusinessTable` | `MultiBusinessTable` | 删 | 2 | TenantByTableController、MultiBusinessDataSeedData |
| 31 | `MultiBusinessSubTable` | `MultiBusinessSubTable` | 删 | 1 | MultiBusinessDataSeedData |
| 32 | `SubLibraryBusinessTable` | `SubLibraryBusinessTable` | 删 | 2 | TenantByDbController、SubBusinessDataSeedData |

---

## 五、删除影响链（施工图）

删一个实体，要同时处理的位置。**漏一处就编译不过或运行期报错。**

| 层 | 位置 | 说明 |
|---|---|---|
| 1 | `MultiAgentDevelop.Model/Models/Xxx.cs` | 实体本身 |
| 2 | `MultiAgentDevelop.Model/Models/RootTkey/XxxRoot.cs` | 只有该实体独有的基类才删（如 `TopicDetailRoot`） |
| 3 | `MultiAgentDevelop.Model/ViewModels/Xxx*.cs` | 例如删 BlogArticle 要评估 `TestRestSharpGetDto`；删 WeChat 要清理 20+ 个 `WeChat*Dto` |
| 4 | `MultiAgentDevelop.IServices/IXxxServices.cs` | 接口 |
| 5 | `MultiAgentDevelop.Services/XxxServices.cs` | 实现 |
| 6 | `MultiAgentDevelop.Repository/*` | 只删专属仓储（现只有 `RoleModulePermissionRepository`），泛型 `BaseRepository` 不动 |
| 7 | `MultiAgentDevelop.Api/Controllers/XxxController.cs` | 控制器 |
| 8 | `MultiAgentDevelop.Common/Seed/DBSeed.cs` | 删掉对应的 `#region Xxx` 种子代码块（**必做，否则反射/强类型引用会编译失败**） |
| 9 | `MultiAgentDevelop.Common/Seed/SeedData/Xxx*SeedData.cs` | 多租户业务表的种子类 |
| 10 | `MultiAgentDevelop.Api/wwwroot/BlogCore.Data.json/Xxx.tsv` | 种子数据文件 |
| 11 | `MultiAgentDevelop.Tasks/QuartzNet/Jobs/Job_Xxx_Quartz.cs` | 依赖被删实体的定时任务 |
| 12 | `MultiAgentDevelop.Extensions/AutoMapper/CustomProfile.cs` | AutoMapper 映射配置 |
| 13 | ~~`MultiAgentDevelop.EventBus/EventBusKafka/*`~~ | **经核实无需改动**：那里的 `Topic` 是 Kafka 配置项 `KafkaOptions.Topic`，与实体 `Topic` 无关（初版文档此处判断有误，已更正） |
| 13.1 | `Api/Filter/UseServiceDIAttribute.cs`、`Extensions/Redis/RedisSubscribe.cs`、`Extensions/EventHandling/BlogQueryIntegrationEventHandler.cs`、`Extensions/HostedService/Job1TimedService.cs` | **易漏项**：这些类通过**构造函数注入了 `IBlogArticleServices`**。因为 `\bBlogArticle\b` 匹配不到 `IBlogArticleServices`，按实体名扫描会漏掉 |
| 14 | `MultiAgentDevelop.Api/Controllers/ValuesController.cs`、`TransactionController.cs` | 框架自带的演示控制器，引用了一堆示例实体，建议一并删空 |
| 15 | `MultiAgentDevelop.Vue3/src/api/*.ts` + `src/views/*` | 前端对应页面（blog/department/login/module/permission/quartzJob/role/user 共 8 个模块） |
| 16 | 数据库表 | `DBSeed` 只建不删，**必须手工 `DROP TABLE`** |
| 17 | `appsettings.json` | `Middleware.QuartzNetJob.Enabled`、`EventBus.Enabled` 等开关 |
| 18 | `BlogCore.Data.json/` 下 `Modules.tsv` / `Permission.tsv` / `RoleModulePermission.tsv` | **三份按 Id 互相关联**：`Permission.Mid→Modules.Id`、`Permission.Pid→Permission.Id`、`RoleModulePermission.ModuleId→Modules.Id`、`RoleModulePermission.PermissionId→Permission.Id`。删菜单必须三份一致处理，否则留下悬空引用。**实测前提：主键非自增（全项目无 `IsIdentity`），故 Id 留空洞安全、无需重排**（详见 C 档"种子菜单数据清理记录"） |

> **不用动的**：`MultiAgentDevelop.Model/Love.cs`。它是空类，被 `AutofacModuleRegister.cs` 第 105 行当程序集锚点用（`Assembly.GetAssembly(typeof(Love))`），删了会编译失败。

---

## 六、分档清理建议

### A 档 — 零风险（已于 2026-10-13 执行完毕）

**已删除的 7 个文件：**

```
MultiAgentDevelop.Model/Models/GblLogAudit.cs            零引用孤儿
MultiAgentDevelop.Model/Models/ReportAbnoraml.cs         零引用孤儿（医疗业务残留）
MultiAgentDevelop.Model/Models/TestModels.cs             测试 DTO，本不该建表
MultiAgentDevelop.Model/Models/SplitDemo.cs              分表演示
MultiAgentDevelop.Api/Controllers/SplitDemoController.cs
MultiAgentDevelop.IServices/ISplitDemoServices.cs
MultiAgentDevelop.Services/SplitDemoServices.cs
```

**同步清理的 5 个引用文件：**

| 文件 | 改动 |
|---|---|
| `MultiAgentDevelop.Repository/RoleModulePermissionRepository.cs` | 删除 `QueryMuchTable()` 方法（含 `TestMuchTableResult` 的联查构造） |
| `MultiAgentDevelop.Repository/IRoleModulePermissionRepository.cs` | 删除 `QueryMuchTable()` 接口声明 |
| `MultiAgentDevelop.Services/RoleModulePermissionServices.cs` | 删除 `QueryMuchTable()` 实现 |
| `MultiAgentDevelop.IServices/IRoleModulePermissionServices.cs` | 删除 `QueryMuchTable()` 接口声明 |
| `MultiAgentDevelop.Api/Controllers/ValuesController.cs` | 删除 demo 调用段（第 189-190 行）以及随之失效的 `_roleModulePermissionServices` 字段/构造注入 |

> `QueryMuchTable()` 的唯一消费者是 `ValuesController` 里一段 demo 代码，赋给局部变量后从未使用，方法返回的是 `data` 而非该变量 —— 属于纯死代码，删除无功能影响。

**编译验证**：`dotnet build MultiAgentDevelop.sln` → **0 错误**，14 个警告（全部为改动前既有），`MultiAgentDevelop.Services.dll` / `Repository.dll` 正常输出到 `Api/bin/Debug/net8.0/`。

**本次未动的遗留项：**

1. `BaseRepository` / `IBaseRepository` / `BaseServices` / `IBaseServices` 里的 5 个泛型分表方法 —— `AddSplit` / `UpdateSplit` / `DeleteSplit` / `QueryByIdSplit` / `QueryPageSplit`。删掉 `SplitDemoController` 后已无调用者，但它们写在公共泛型基类上，属框架能力（任何实体加 `[SplitTable]` 即可用），故保留。
2. 数据库中 `GblLogAudit`、`REPORT_ABNORAML`、`TestMuchTableResult`、`SplitDemo_*` 这几张表**需手工 DROP** —— `DBSeed` 只建不删。


### B 档 — 演示业务（已于 2026-10-13 执行完毕）

需求确认：**系统不开发博客、话题、留言、密码库业务。**

**已删除的 31 个后端文件 + 5 个前端文件：**

| 层 | 文件 |
|---|---|
| 实体（7） | `Models/BlogArticle.cs`、`BlogArticleComment.cs`、`Topic.cs`、`TopicDetail.cs`、`Guestbook.cs`、`PasswordLib.cs`、`RootTkey/TopicDetailRoot.cs` |
| ViewModel（4） | `ViewModels/BlogViewModels.cs`、`GuestbookViewModels.cs`、`TestRestSharpGetDto.cs`、`TopgbViewModels.cs` |
| 控制器（4） | `BlogController.cs`、`TopicController.cs`、`TopicDetailController.cs`、`TransactionController.cs` |
| 服务（10） | `IBlogArticleServices` / `ITopicServices` / `ITopicDetailServices` / `IGuestbookServices` / `IPasswordLibServices` 及各自实现 |
| 定时任务（1） | `Tasks/QuartzNet/Jobs/Job_Blogs_Quartz.cs` |
| 测试（3） | `Tests/Controller_Test/BlogController_Should.cs`、`Service_Test/BlogArticleService_Should.cs`、`Common_Test/DynamicLambdaTest.cs` |
| 种子数据（3） | `wwwroot/BlogCore.Data.json/BlogArticle.tsv`、`Topic.tsv`、`TopicDetail.tsv` |
| 前端（5） | `Vue3/src/api/blogApi.ts`、`src/views/Blog/{Blogs.vue,Detail.vue,blogsFunctions.ts}`、`src/views/Tibug/Bugs.vue` |

**同步改造的 11 个文件（否则编译不过或留死代码）：**

| 文件 | 改动 |
|---|---|
| `Common/Seed/DBSeed.cs` | 删除 `#region BlogArticle` / `#region Topic` / `#region TopicDetail` 三个种子块 |
| `Extensions/AutoMapper/CustomProfile.cs` | 删除 `BlogArticle ↔ BlogViewModels` 两行映射 |
| `Api/Controllers/ValuesController.cs` | 删除 `_blogArticleServices` / `_passwordLibServices` 字段与构造注入；删除 `TestSqlsugarWithCache()`、`Get()`、`Test_Aop_Cache()`、`TestMutiDBAPI()` 四个纯博客方法；`Post()` 的 body 参数由 `BlogArticle` 改为 `Role` |
| `Api/Filter/UseServiceDIAttribute.cs` | `IBlogArticleServices` → `IRoleServices` |
| `Extensions/Redis/RedisSubscribe.cs` | 删除未被使用的 `IBlogArticleServices` 注入（订阅方法里根本没用到） |
| `Extensions/EventHandling/BlogQueryIntegrationEventHandler.cs` | 删除 `IBlogArticleServices` 依赖，`Handle` 只保留日志；事件总线骨架保留 |
| `Extensions/HostedService/Job1TimedService.cs` | `IBlogArticleServices` → `ISysUserInfoServices`，输出改为用户登录名 |
| `Tasks/HostedService/Job1TimedService.cs` | **整个删除**（与 Extensions 那份重复，且从未被 `AddHostedService` 注册，属死代码） |
| `Tests/Repository_Test/OrmTest.cs` | 测试载体 `BlogArticle`/`BlogArticleComment` → `SysUserInfo`/`UserRole` |
| `Tests/Repository_Test/Repository_Base_Should.cs` | 测试载体 → `SysUserInfo`（保留仓储基类 CRUD 覆盖） |
| `Api/MultiAgentDevelop.Api.csproj` | 删除 3 个已删 tsv 的 `<None Include>` 条目 |

**编译验证**：`dotnet build MultiAgentDevelop.sln` → **0 错误**，12 个警告（均为既有），13 个程序集全部生成。

**B 档遗留（需你决定）：**

1. **种子菜单数据仍指向已删接口**：`wwwroot/BlogCore.Data.json/Modules.tsv` 里有 7 条（3 条 `/api/TopicDetail/*` + 4 条 `/api/Blog*`），`Permission.tsv` 里有 2 条前端路由（`/Blog/Blogs`、`/Blog/Detail/:id`），`RoleModulePermission.tsv` 里有对应的关联记录。
   - 这三份文件**当前不生效**（`AppSettings.SeedDBDataEnabled = false`）。
   - 未自动清理的原因：`Permission.tsv` 是 **JSONC 格式**（内含 `//{ ... }` 注释块），程序化改写风险高；且三者按 `Id` 互相关联，必须一致删除才不会留下悬空记录。
2. 数据库里 `BlogArticle`、`BlogArticleComment`、`Topic`、`TopicDetail`、`Guestbook`、`PasswordLib` 六张表**需手工 DROP**。

### C 档 — 独立功能模块（已执行：微信删 / 多租户演示删 / Quartz 保留）

- ~~**微信模块**~~ **已于 2026-10-13 整块删除**，共 44 个文件（详见下方"微信模块执行记录"）。
- ~~**多租户演示**~~ **已于 2026-10-13 删除**，共 11 个文件 + 1 个空目录（详见下方"多租户演示执行记录"）。
  保留了 `SysTenant`、`TenantManagerController`、`TenantSeedData` —— 它们是租户能力的本体，不是演示数据。
- **Quartz 定时任务**：`TasksQz` / `TasksLog` + `TasksQzController` + `SchedulerCenterServer` + 4 个 Job。
  **2026-10-13 决定保留。** 它是基础设施而非演示数据，删了以后没法跑任何定时任务，`JobBase` 也依赖这两张表。

#### 微信模块执行记录（2026-10-13）

**删除 44 个文件：**

| 层 | 数量 | 文件 |
|---|---|---|
| 实体 | 5 | `Models/WeChatCompany.cs`、`WeChatConfig.cs`、`WeChatPushLog.cs`、`WeChatQR.cs`、`WeChatSub.cs` |
| ViewModel | 25 | `ViewModels/WeChat*.cs` 全部 |
| 控制器 | 5 | `WeChatController`、`WeChatCompanyController`、`WeChatConfigController`、`WeChatPushLogController`、`WeChatSubController` |
| 服务 | 8 | 4 组 `IWeChat*Services` + 4 组实现 |
| 工具类 | 1 | `Common/Helper/WeChatHelper.cs`（310 行） |

**改动说明：**

- **零改造，纯删除。** 这 44 个文件是一个闭环：微信实体只被微信自己的 Service 引用，`WeChat*Dto` 只被微信 Service/Controller/Helper 引用，外部零依赖 —— 所以没有任何一处需要改代码，这点和 A/B 档不同。
- `WeChatHelper.cs` 本身不依赖实体（只依赖 `Model.ViewModels` 里的 DTO），唯一调用者是 `WeChatConfigServices`。删掉 Service 后它成死代码，故一并删除。**它是独立的公众号 API 封装，以后若要对接公众号，可从 git 历史单独恢复。**
- 注意是 **4 组** Service 不是 5 组 —— `WeChatQR` 没有独立服务，被 `WeChatConfigServices` 内部使用。

**编译验证**：`dotnet build MultiAgentDevelop.sln` → **0 错误**，12 个警告（均为既有）。全局 `WeChat` 关键字扫描 → **0 残留**。

**待办**：数据库 5 张表 `WeChatCompany` / `WeChatConfig` / `WeChatPushLog` / `WeChatQR` / `WeChatSub` 需手工 DROP。

#### 多租户演示执行记录（2026-10-13）

**删除 11 个文件 + 1 个空目录：**

| 层 | 数量 | 文件 |
|---|---|---|
| 实体 | 4 | `Models/Tenant/BusinessTable.cs`、`MultiBusinessTable.cs`、`MultiBusinessSubTable.cs`、`SubLibraryBusinessTable.cs` |
| 控制器 | 3 | `Api/Controllers/Tenant/TenantByDbController.cs`、`TenantByIdController.cs`、`TenantByTableController.cs` |
| 种子类 | 4 | `Common/Seed/SeedData/BusinessDataSeedData.cs`、`MultiBusinessDataSeedData.cs`、`MultiBusinessSubDataSeedData.cs`、`SubBusinessDataSeedData.cs` |

**⚠️ 本次最容易踩的坑：`TenantManagerController` 管的不是演示业务，是 `SysTenant` 核心表。**

它和 3 个 `TenantBy*Controller` 放在同一个 `Controllers/Tenant/` 目录里，名字也像，但它操作的是 `SysTenant`（即租户能力的本体，被 `SysUserInfo.TenantId` 以 OneToOne 真导航引用）。**删演示表时必须把它留下**，否则要连带改 `SysUserInfo`、`TenantUtil`、`RepositorySetting`。同理保留 `TenantSeedData.cs`（建默认租户）和 `UserInfoSeedData.cs`（建默认管理员）。

**保留清单**（`Common/Seed/SeedData/` 清理后只剩这两个）：

```
TenantSeedData.cs      建默认租户（核心）
UserInfoSeedData.cs    建默认管理员（核心）
```

**为什么是纯删除、零改造：**

四张演示表只被那 3 个演示 Controller 和 4 个种子类引用，外部零依赖。它们分两条路建表：

- `BusinessTable` 是 `ITenantEntity`（字段隔离），**不带** `[MultiTenant]` → 走 `DBSeed.SeedDataAsync`，会被反射建表逻辑捕获。
- 其余三张带 `[MultiTenant(TenantTypeEnum.Tables)]` / `[MultiTenant]`（多库） → 走 `TenantSeedAsync` → `TenantUtil.GetTenantEntityTypes()`，**不走**反射建表逻辑。

两条路在删掉实体后都会自然返回空集（`if (!entityTypes.Any()) return;`），所以 `DBSeed.cs` 和 `TenantService.cs` **一行都不用改**。

**编译验证**：`dotnet build MultiAgentDevelop.sln` → **0 错误**。演示表 / 演示 Controller / 演示种子类关键字全局扫描 → **0 残留**。

**待办**：数据库 4 张表 `BusinessTable` / `MultiBusinessTable` / `MultiBusinessSubTable` / `SubLibraryBusinessTable` 需手工 DROP（其中后三张可能是**按租户后缀**的多张表，需按前缀批量处理）。

#### 种子菜单数据清理记录（2026-10-13）

清理对象：`MultiAgentDevelop.Api/wwwroot/BlogCore.Data.json/` 下的三份 `tsv`（实际内容是 JSON 数组）。

| 文件 | 清理前 | 清理后 | 删除 |
|---|---|---|---|
| `Modules.tsv` | 74 | 49 | **25**（博客/话题 7 + 微信 18） |
| `Permission.tsv` | 123 | 86 | **37** |
| `RoleModulePermission.tsv` | 240 | 180 | **60** |

**一个决定性前提：全项目主键都不是自增的。**

`RootEntityTkey<Tkey>.Id` 只标了 `[SugarColumn(IsNullable = false, IsPrimaryKey = true)]`，**没有 `IsIdentity`**（全局 grep 确认：整个 `Model` 项目里 `IsIdentity` 只出现一次，且在被注释掉的代码里）。所以 `Id` 是代码显式赋值的。

→ **删除条目后 `Id` 留空洞完全安全，所有引用关系天然保持正确，不需要重排 Id。** 这一点如果判断反了（误以为自增），就会去重排 Id 并同步改引用，反而引入大面积风险。

**Permission.tsv 是三个目录树整体删除**（做了 Pid 闭包计算，不存在"父目录保留、子节点删光"的残缺情况）：

```
问题管理   (Id 42) → TiBug /Tibug/Bugs (28) → 编辑/删除/查询 (30,31,32)
博客管理   (Id 29) → 博客 /Blog/Blogs (43) → 删除/查询/编辑 (64,73,74)
                   → 博客详情 /Blog/Detail/:id (67)
微信公众号管理(Id 87) → 微信列表/客户/菜单/模板消息/推送记录/订阅用户/绑定用户/文本消息 (88~95)
                     → 其下全部按钮级权限 (96,98~113)
```

**顺带清掉了两处遗留：**

1. **一段被注释掉的重复条目**：原文件 2341-2362 行有 `//{ "Id": 114, "Name": "推送文字消息" ... }`，`Pid=95`（属微信目录树）。它被注释的原因很可能就是 **`Id` 与「部门权限管理」冲突**（两者都是 114）。删掉后 `Id 114` 唯一对应「部门权限管理」。
2. **一处多余空行**：`Modules.tsv` 中 `Id 46` 与 `Id 47` 之间有连续两个空行（当年追加微信条目留下的），规范化为一处。

**写入方式与校验：**

- 采用**文本级精确切除**（带字符串状态机的括号匹配定位顶层元素），而非重新序列化 —— 保证 git diff 只显示被删条目，不因格式化产生整文件变动。
- 切除前先做**无损性自检**：用同一套拼接逻辑重建"零删除"版本，逐行比对必须与原文一致，通过后才写入。三份文件均通过（`Modules.tsv` 仅上述 1 处空行规范化）。
- 写入前一并备份到 `BlogCore.Data.json/_backup_20261013/`（三个原始文件）。

**引用完整性校验结果：**

| 检查项 | 结果 |
|---|---|
| 待删 Id 残留 | 0 ✓ |
| `Permission.Mid` → `Modules` 悬空 | 0 ✓ |
| `Permission.Pid` → `Permission` 悬空 | 0 ✓ |
| `RoleModulePermission.PermissionId` 悬空 | 0 ✓ |
| `RoleModulePermission.RoleId` 悬空 | 0 ✓ |
| **真正的悬空引用（清理前 vs 清理后）** | **0 → 0 ✓** |

**两个需要说明的"非问题"：**

1. `RoleModulePermission` 里有 69 条 `ModuleId = 0`，看着像悬空，其实是**"仅绑权限、不绑接口模块"的设计特征**（配 `PermissionId` 使用）。清理前是 88 条，减少的 19 条正是被删的微信按钮级授权。
2. `Permission.tsv` 里有 14 条 `Name = "s"`、`Code = "s"` 的垃圾数据（Id 50~63，框架原作者遗留），**不在本次清除范围，原样保留**。

### D 档 — 必留（不要动）

```
SysUserInfo / SysUserInfoRoot     用户
Role                              角色
Permission / PermissionRoot       菜单权限
Modules / ModulesRoot             接口模块
RoleModulePermission (+Root)      权限枢纽
UserRole / UserRoleRoot           用户角色
Department / DepartmentRoot       部门（数据权限）
RootEntityTkey / BaseEntity / IDeleteFilter    基类
Love                              程序集锚点
SysTenant                         若用多租户则保留
OperateLog / AccessTrendLog       若保留后台统计则保留
```

---

## 七、建议执行顺序

1. ~~先删 A 档（4 个类 + 1 组 Service），编译通过验证一遍。~~ **已完成，0 错误。**
2. ~~删 B 档（博客/话题/留言/密码库），每删一块编译一次。~~ **已完成，0 错误。**
3. ~~决定 C 档范围（微信 / 多租户演示 / Quartz），按模块整块删。~~ **已完成** —— 微信整块删、多租户演示删、Quartz 保留。
4. ~~清理种子菜单数据（`Modules.tsv` / `Permission.tsv` / `RoleModulePermission.tsv`）。~~ **已完成**，共删 122 条，校验无悬空引用。
5. **手工 `DROP` 数据库里的旧表**：`DBSeed` 只建不删。**待你处理** —— 涉及 A 档 4 张 + B 档 6 张 + 微信 5 张 + 多租户演示 4 张。
6. **最后跑一次 `dotnet build`（或 VS 的 F6）**，确认 `MultiAgentDevelop.Services.dll` / `Repository.dll` 正常生成——Autofac 启动时会硬检查这两个 dll 是否存在。**已在每轮清理后验证通过。**

---

## 八、待你确认

1. ~~C 档剩余范围~~ **已确定并执行完毕**：微信整块删、多租户演示删、Quartz 保留。
2. **`SysTenant` 是否要删**（未做）：它被 `SysUserInfo.TenantId` 以 OneToOne 真导航引用，删它需同步改 `SysUserInfo`、`TenantUtil`、`RepositorySetting`。当前**已保留**。
3. ~~B 档的种子菜单数据~~ **已清理**（含微信菜单残留，共删 122 条）。原始文件备份在 `BlogCore.Data.json/_backup_20261013/`。
4. **数据库旧表 `DROP`**（未做，你表示自行处理）。
5. **项目实际业务实体**：`Model/Models` 下目前仍无业务实体。若打算从现有 Oracle 库（当前 `Main` 连接指向 `C##ZHOUSHAN_TIJIAN@health`）反向生成，可用 `DbFirstController`。
6. **发现但未动的可选清理项**：
   - `Permission.tsv` 里 14 条 `Name="s"` / `Code="s"` 的垃圾菜单（Id 50~63，框架原作者遗留）。
   - `Modules.tsv` 里若干指向历史演示接口的条目：`/api/values`(Id 1)、`/api/claims`(2)、`/api/UserRole`(3)、`/api/v2/Apb/apbs`(4)。其中 `/api/Agent/get`(34)、`/api/test/get`(35) 已被原作者标为 `IsDeleted=1`。
   - `_backup_20261013/` 备份目录，你核对无误后可自行删除。
