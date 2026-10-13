# 多 Agent 虚拟开发团队 — 协作总纲

本文档是短信中台项目的多 Agent 协作制度，定义角色职责、交接物和状态流转。所有 Agent 开工前必须先读本文件和根目录 `AGENT.md`。

## 角色

| 角色 | 代号 | 职责 | 输入 | 输出 |
|---|---|---|---|---|
| 产品经理 | PM | 按菜单功能点输出功能详细说明文档（PRD） | 业务需求（褚哥口述/补充） | `docs/prd/NN-功能名.md` |
| 项目经理 | PMS | 根据 PRD 制定任务清单，维护开发/测试状态 | PRD 文档 | `docs/todoList.md` |
| 开发工程师 | DEV | 从 todoList 领取 1 个任务编码，遵循框架分层约定 | todoList + PRD + AGENT.md | 代码 + 单元测试，更新 todoList 状态 |
| 测试工程师 | QA | 模拟用户登录系统，对照 PRD 验收功能 | PRD + 可运行的系统 | `docs/test/NN-功能名.md` 验收报告，更新 todoList 状态 |

## 交接物与目录约定

```
docs/
  team.md              # 本文件（协作制度）
  todoList.md          # 项目任务清单（PMS 维护，全员读写状态列）
  prd/
    _TEMPLATE.md       # PRD 模板
    NN-功能名.md       # 各功能 PRD（NN 为两位序号）
  test/
    NN-功能名.md       # 各功能验收报告（QA 产出）
```

## 任务状态机

`docs/todoList.md` 中每个任务的状态按以下流转，**谁执行谁更新**：

```
待开发 → 开发中(DEV) → 待测试(DEV完成后) → 测试通过(QA验收) → [已完成]
                              ↓ 测试不通过
                          失败返工(DEV修复后改回 待测试)
```

## 标准工作流（一个功能的完整生命周期）

1. **PM**：褚哥给出功能点 → PM 用 `docs/prd/_TEMPLATE.md` 产出 `docs/prd/NN-功能名.md`，含页面布局、字段定义、业务规则、接口契约、数据库表、验收标准 → 通知 PMS。
2. **PMS**：读 PRD → 在 `docs/todoList.md` 拆解为后端/前端/联调测试任务（每任务标注对应 PRD 序号）→ 状态=待开发 → 通知 DEV。
3. **DEV**：按优先级领取 1 个任务 → 状态改为 `开发中` → 按 `AGENT.md` 分层约定编码（Model → IServices → Services → Repository → Api Controller → Vue3 页面/api/路由）→ 完成 `dotnet build` + 相关测试通过 → 状态改为 `待测试`，并在 todoList 备注 PRD 序号 → 通知 QA。
4. **QA**：启动前后端（`dotnet run --project .\MultiAgentDevelop.Api` + `npm run dev --prefix .\MultiAgentDevelop.Vue3`）→ 用测试账号登录 → 逐条核对 PRD「验收标准」→ 产出 `docs/test/NN-功能名.md`（逐条 通过/不通过 + 截图说明）→ 全部通过改状态 `测试通过`，有不通过改 `失败返工` 并在报告中列明缺陷 → 通知 DEV。
5. **DEV**：读 QA 报告修复 → 改回 `待测试` → 循环直到 QA 全部通过。

## 硬性约定

- 编码必须遵循根目录 `AGENT.md` 的分层与依赖方向，优先复用 `Extensions` 中已有 `Add*Setup` 扩展。
- 数据库：Oracle（主库 ConnId=Main），表名 snake_case，字段属性 PascalCase，布尔用 NUMBER(1)，逻辑删除统一 `is_deleted`。
- 实体继承 `RootEntityTkey<long>`，参考 `MultiAgentDevelop.Model/Models/OperateLog.cs` 写法。
- API 契约遵循现有统一响应结构（ApiResponse）与 Newtonsoft.Json 序列化配置（见 AGENT.md）。
- 新页面落到 `MultiAgentDevelop.Vue3/src/views/`，接口封装到 `src/api/`，路由注册到 `src/router/`。
- DEV 完成任务必须以 `dotnet build .\MultiAgentDevelop.sln` 零错误为完成标准；涉及前端必须 `npm run build` 通过。
- 不允许跳过 QA 直接标记完成；QA 不允许修改业务代码。
- 菜单权限：新功能需在系统菜单中注册（Module），由 DEV 在任务内完成种子数据或菜单管理页面录入。
