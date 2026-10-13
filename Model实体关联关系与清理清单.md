# MultiAgentDevelop.Model 实体关联关系与清理清单

> 分析范围：`MultiAgentDevelop.Model/Models/`（含 `Models/Tenant/`、`Models/RootTkey/`）
> 分析基准：整个解决方案的源码引用（排除 `bin/`、`obj/`、`MultiAgentDevelop.Vue3/` 前端与 `MultiAgentDevelop.Tests/` 测试项目）
> 结论：41 个 .cs 类 = 8 个框架基类 + 1 个接口 + 32 个实体。其中 **2 个是零引用孤儿**，**22 个是框架自带的演示业务实体**。

---

## 一、结论先行

| 分类 | 数量 | 处理建议 |
|---|---|---|
| 身份权限核心（登录/角色/菜单/部门） | 9 | **必留**，删了系统直接跑不起来 |
| 框架基类（RootTkey / BaseEntity） | 8 | **必留**，所有实体都继承它们 |
| 孤儿实体（0 引用） | 2 | **直接删**，无任何代码依赖 |
| 演示业务（博客/话题/留言/密码库） | 6 | 可删，需连带清理 Controller/Service/Job/tsv |
| 微信模块 | 5 | 可删，整块独立 |
| 定时任务（Quartz） | 2 | 可删（但删了就没法跑定时任务） |
| 多租户（SysTenant + 4 张业务演示表） | 5 | 看是否用多租户 |
| 分表演示 | 1 | 可删 |
| 测试 DTO（会被误建表） | 1 | **建议删**，它是查询结果类，不该建成表 |

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

### 3.3 演示业务（Blog.Core 自带示例）

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

### 3.4 微信模块（整块独立，5 张表）

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

无实体导航属性，全部靠字符串 ID 手工关联。引用者：5 个 Controller + 5 个 Service，各成一套。
注意：**前端 `MultiAgentDevelop.Vue3/src/api/` 下没有微信相关的 api 文件**，说明前端没接这套后端接口。

### 3.5 定时任务（Quartz，2 张表）

```
TasksQz (任务定义: Cron / AssemblyName / ClassName / JobGroup)
    └── TasksLog.JobId → TasksQz.Id   (执行日志，弱关联)
```

- `TasksQz` 被 `TasksQzController`、`SchedulerCenterServer`、`ISchedulerCenter`、`JobBase` 使用
- `TasksLog` 被 `TasksQzController`、`JobBase`（所有 Job 的基类）使用
- 相关的 `Job_*` 类共 5 个：`Job_Blogs_Quartz`、`Job_AccessTrendLog_Quartz`、`Job_OperateLog_Quartz`、`Job_URL_Quartz`
- **当前开关是关闭的**：`appsettings.json` → `Middleware.QuartzNetJob.Enabled = false`

> 删 Task 表之前想清楚：`JobBase` 是基础设施，但它强依赖 `ITasksQzServices` / `ITasksLogServices`。删了就没法再跑任何定时任务。

### 3.6 多租户（5 张表，看是否真的要用）

```
SysTenant (租户定义: TenantType / ConfigId / Connection)
    ├── SysUserInfo.TenantId → SysTenant.Id   ← 唯一的代码级强关联
    └── TenantUtil / RepositorySetting 全局过滤器

演示业务表（BaseEntity 子类）：
    BusinessTable          [ITenantEntity]          → 按租户字段隔离（TenantByIdController）
    MultiBusinessTable     [MultiTenant(Tables)]    → 按租户分表（TenantByTableController）
        └── MultiBusinessSubTable.MainId → MultiBusinessTable.Id
    SubLibraryBusinessTable [MultiTenant]           → 按租户分库（TenantByDbController）
```

三个 `Tenant*Controller` + `TenantManagerController` + `TenantService` 是完整的演示代码。
真正被生产链路使用是的 `SysTenant`（`SysUserInfo` 的 OneToOne 导航 + `TenantUtil` 全局过滤器），4 张业务表纯粹是演示。

### 3.7 孤儿实体（零引用，可直接删）

| 实体 | 表名 | 引用情况 | 判断 |
|---|---|---|---|
| `GblLogAudit` | `GblLogAudit` | **全解决方案 0 引用** | 类注释写的是"用户团队表"（明显是从别处拷来的），字段是日志审计（TraceId/Level/Logger/LogType）。框架遗留，无任何代码使用 |
| `ReportAbnoraml` | `REPORT_ABNORAML` | **全解决方案 0 引用** | 字段是 `ORGAN_ID` / `REPORT_ID` / `ABNORMAL_CODE` / `MEDICAL_EXPLAIN`（医学解释）/ `MEDICAL_LIMIT`（就医期限），是**医疗健康业务实体**，从 SeeHealth 那套库里带过来的残留 |

---

## 四、完整实体清单与引用矩阵

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
| 13 | `MultiAgentDevelop.EventBus/EventBusKafka/*` | 删 `Topic` 时要同步改（Kafka 示例里用了它） |
| 14 | `MultiAgentDevelop.Api/Controllers/ValuesController.cs`、`TransactionController.cs` | 框架自带的演示控制器，引用了一堆示例实体，建议一并删空 |
| 15 | `MultiAgentDevelop.Vue3/src/api/*.ts` + `src/views/*` | 前端对应页面（blog/department/login/module/permission/quartzJob/role/user 共 8 个模块） |
| 16 | 数据库表 | `DBSeed` 只建不删，**必须手工 `DROP TABLE`** |
| 17 | `appsettings.json` | `Middleware.QuartzNetJob.Enabled`、`EventBus.Enabled` 等开关 |

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


### B 档 — 演示业务（确认不做博客/话题就删）

```
BlogArticle / BlogArticleComment / Topic / TopicDetail / Guestbook / PasswordLib
```
连带：6 个 Controller（Blog、Topic、TopicDetail）+ 6 组 Service/IService + 4 个 Job 里对它们的引用 + `CustomProfile` 映射 + `BlogCore.Data.json/*.tsv`（8 个文件）+ `ValuesController` / `TransactionController`。
注意 `Topic` 还被 `EventBus/EventBusKafka` 三处引用，一起改。

### C 档 — 独立功能模块（按需删）

- **微信模块**：`WeChat*` 5 个实体 + 5 个 Controller + 5 组 Service + `ViewModels/WeChat*Dto.cs` 20 余个。前端没接，整块删干净。
- **多租户演示**：`BusinessTable` / `MultiBusinessTable` / `MultiBusinessSubTable` / `SubLibraryBusinessTable` + 3 个 Tenant 演示 Controller + 4 个 SeedData 类。
  ⚠️ `SysTenant` 要不要删是另一回事——`SysUserInfo` 上有 `TenantId` 字段和 OneToOne 导航，删 `SysTenant` 要同步改 `SysUserInfo`、`TenantUtil`、`RepositorySetting`。
- **Quartz 定时任务**：`TasksQz` / `TasksLog` + `TasksQzController` + `SchedulerCenterServer` + 4 个 Job。**删了以后没法跑任何定时任务**，`JobBase` 也依赖这两张表。

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
2. **决定 B/C 档范围**，按模块整块删，每删一块编译一次。
3. **清 `DBSeed.cs` 里的 `#region`** 和 `wwwroot/BlogCore.Data.json/*.tsv`。
4. **删 `ValuesController.cs` / `TransactionController.cs`**（框架演示控制器，没有业务价值）。
5. **清前端 `src/api/*.ts` 和 `src/views/*`** 里对应模块。
6. **手工 `DROP` 数据库里的旧表**：`DBSeed` 只建不删。
7. 最后跑一次 `dotnet build`（或 VS 的 F6），确认 `MultiAgentDevelop.Services.dll` / `Repository.dll` 正常生成——Autofac 启动时会硬检查这两个 dll 是否存在。

---

## 八、待你确认的两件事

1. **这个项目实际要做什么业务？** 目前 `Model/Models` 下**没有任何业务实体**，全是框架自带的演示表。如果目标是从你自己的 Oracle 库反向生成实体，那 B/C 档可以一次性全删，然后用 `DbFirstController` 从 `Main` 连接（当前指向 `C##ZHOUSHAN_TIJIAN@health`）反向生成。
2. **多租户和 Quartz 要不要留？** 这两个不删则要保留对应的 Controller/Service/Seed 一整条链。
