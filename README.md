<img align="right" height="50px" src="http://apk.neters.club/logocore.png">

# MultiAgentDevelop

[English](README-en.md) | 简体中文

[![sdk](https://img.shields.io/badge/sdk-6.0.1-d.svg)](#)  [![Build status](https://github.com/anjoy8/multiagentdevelop/workflows/.NET%20Core/badge.svg)](https://github.com/anjoy8/MultiAgentDevelop/actions)  [![Build Status](https://dev.azure.com/laozhangisphi/anjoy8/_apis/build/status/anjoy8.MultiAgentDevelop?branchName=master)](https://dev.azure.com/laozhangisphi/anjoy8/_build?definitionId=1)  [![codecov](https://codecov.io/gh/anjoy8/MultiAgentDevelop/branch/master/graph/badge.svg)](https://codecov.io/gh/anjoy8/MultiAgentDevelop)  [![License MIT](https://img.shields.io/badge/license-Apache-blue.svg?style=flat-square)](https://github.com/anjoy8/MultiAgentDevelop/blob/master/LICENSE) [![star this repo](http://githubbadges.com/star.svg?user=anjoy8&repo=multiagentdevelop&style=flat)](https://github.com/boennemann/badges)  [![fork this repo](http://githubbadges.com/fork.svg?user=anjoy8&repo=multiagentdevelop&style=flat)](https://github.com/boennemann/badges/fork)  [![博客园](https://img.shields.io/badge/博客园-老张的哲学-brightgreen.svg)](https://www.cnblogs.com/laozhang-is-phi/)

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=anjoy8/multiagentdevelop&type=Date)](https://star-history.com/#anjoy8/multiagentdevelop&Date)


&nbsp;
&nbsp;

 
<div style="text-align: center;">
<a href="https://mvp.microsoft.com/zh-cn/PublicProfile/5003704?fullName=anson%20zhang" >
  <img src="http://apk.neters.club/MVP_Logo_Horizontal_Preferred_Cyan300_CMYK_72ppi.png" alt="MVP"   >
</a>

 
</div>

-------------------------------
MultiAgentDevelop 开箱即用的企业级前后端分离【 .NET Core6.0 Api + Vue 2.x + RBAC】权限框架。  
其他版本看具体分支吧🎉  
官网：http://apk.neters.club/.doc/    

  
---------------------

**已被近100家公司所使用(🐱‍🚀)：[点击查看列表](https://github.com/anjoy8/MultiAgentDevelop/issues/75)**   欢迎盖楼，留下公司真实名字的，可得定制化指导服务。  
同时如果企业有付费咨询，欢迎联系老张（QQ：3143422472）。
  

### 核心项目组成员（排名不分先后）

[hudingwen](https://github.com/hudingwen)、[LemonNoCry](https://github.com/LemonNoCry)、、[Jamnine何拾玖](https://github.com/Jamnine)、


#### ❤ 真实用户反馈 ❤
```
项目单体部署，并发在400~500，一切正常(不保证自己的各种错误写法)。  
如果搭配负载，效果更好。

1、A~CoderDong:
应用场景：使用MultiAgentDevelop为基础骨架开发，搭建Client监控类守护进程项目，To C 客户群，
并发情况：目前压测并发5k正常8秒处理完，并发10k可15秒处理完毕，异常不会丢失。
生产配置：一台服务器（Linux环境 + 至强8核的16G内存 + mysql数据库 + 3台Nginx负载）

```
   
 
 
&nbsp;

## 给个星星! ⭐️
如果你喜欢这个项目或者它帮助你, 请给 Star~   
如果你的项目中借鉴了本项目，请稍微说明下[https://github.com/anjoy8/MultiAgentDevelop/issues/75](https://github.com/anjoy8/MultiAgentDevelop/issues/75)，开源不易✨。  



### 功能与进度

#### 企业使用高级版本

- [x] 包含开源版 `框架模块/组件模块` 中的所有功能；
- [x] 全部表结构主键底层架构改成`string`类型（默认雪花，支持guid），更方便迁移；
- [x] 完善部门数据权限，可以基于策略配置查看数据范围；
- [x] 优化权限处理器，解决多实例分布式下，权限不同步问题（必须配置Redis）；
- [x] 增加在线用户查看功能，并实现强制用户下线功能（必须配置Redis）；
- [x] 增加用户黑名单功能（必须配置Redis）；
- [x] 增加岗位功能（单独建表），配合部门使用；
- [ ] 后期优化站内通知功能，其实目前已经有SignalR来实现消息推送了，可以直接用；
- [ ] 前端`Blog.Admin.Pro`使用`AntDesignVue`框架（设计中，未完全实现）；
- [x] 铁粉奖励：如果参与上述功能和其他付费功能开发，可半价获取商业授权；



#### 框架模块：  
- [x] 采用`仓储+服务+接口`的形式封装框架；
- [x] 自定义项目模板 `CreateYourProject.bat` ，可以一键生成自己的项目；🎶  
- [x] 异步 async/await 开发；  
- [x] 接入国产数据库ORM组件 —— SqlSugar，封装数据库操作，支持级联操作；
- [x] 支持自由切换多种数据库，MySql/SqlServer/Sqlite/Oracle/Postgresql/达梦/人大金仓；
- [x] 实现项目启动，自动生成种子数据 ✨； 
- [x] 实现数据库主键类型配置化，什么类型都可以自定义 ✨； 
- [x] 五种日志记录，审计/异常/请求响应/服务操作/Sql记录等,并自动持久化到数据库表🎶； 
- [x] 支持项目事务处理（若要分布式，用cap即可）✨；
- [x] 设计4种 AOP 切面编程，功能涵盖：日志、缓存、审计、事务 ✨；
- [x] 全局统一封装 Serilog 生成多种日志，并自动生成到数据库中，目前支持MySql/SqlServer/Sqlite/Oracle/Postgresql🎉；
- [x] 设计并支持按钮级别的RBAC权限控制，同时支持一键同步接口和菜单 🎶；
- [x] 支持 T4 代码模板，自动生成每层代码；
- [x] 或使用 DbFirst 一键创建自己项目的四层文件（支持多库）；
- [x] 封装`MultiAgentDevelop.Webapi.Template`项目模板，一键重建自己的项目 ✨；
- [x] 搭配多个前端案例供参考和借鉴：Blog.Vue、Blog.Admin、Nuxt.tbug、Blog.Mvp.Blazor ✨；
- [x] 统一集成 IdentityServer4 认证 ✨;
- [x] 统一实现多租户;
- [x] 实现分表案例，支持分表的增删改查哈分页查询，具体查看SplitDemoController.cs;
- [x] 支持signalR对指定用户通讯;  


组件模块：
- [x] 提供 Redis 做缓存处理；
- [x] 使用 Swagger 做api文档；
- [x] 使用 MiniProfiler 做接口性能分析 ✨；
- [x] 使用 Automapper 处理对象映射；  
- [x] 使用 AutoFac 做依赖注入容器，并提供批量服务注入 ✨；
- [x] 支持 CORS 跨域；
- [x] 封装 JWT 自定义策略授权；
- [x] 使用 Serilog 日志框架，集成原生 ILogger 接口做日志记录；
- [x] 使用 SignalR 双工通讯 ✨；
- [x] 添加 IpRateLimiting 做 API 限流处理;
- [x] 使用 Quartz.net 做任务调度（目前单机多任务，集群调度暂不支持）;
- [x] 支持 数据库`读写分离`和多库操作 ✨;
- [x] 新增 Redis 消息队列 ✨;
- [x] 新增 RabbitMQ 消息队列 ✨;
- [x] 新增 EventBus 事件总线 ✨;
- [x] 新增 - 统一聚合支付;
- [x] 新增 - Nacos注册中心配置;
- [x] 新增 - ES 搜索配置;
- [x] 新增 - Apollo 配置;
- [x] 新增 Kafka 消息队列，并配合实现EventBus ✨;
- [x] 新增 微信公众号管理，并集成到Blog.Admin后台 ✨;
- [x] 新增 - 数据部门权限;
- [x] 新增 - Serilog 集成日志数据持久化到数据库;  
- [x] 新增 - 多租户模式（单表，多表，多库三种模式）;  


微服务模块：
- [x] 可配合 Docker 实现容器化；
- [x] 可配合 Jenkins 实现CI / CD；
- [x] 可配合 Consul 实现服务发现；
- [x] 可配合 Nacos 实现服务发现；
- [x] 可配合 apisix/Ocelot 实现网关处理；
- [x] 可配合 Nginx  实现负载均衡；
- [x] 可配合 Ids4   实现认证中心；

### 核心业务模块
#### 框架采用泛型仓储模式，以下几层为核心层，不可删除    
`MultiAgentDevelop.Api`、`MultiAgentDevelop.Common`、`MultiAgentDevelop.IServices`、`MultiAgentDevelop.Model`、`MultiAgentDevelop.Repository`、`MultiAgentDevelop.Services`、`MultiAgentDevelop.Tasks`、`MultiAgentDevelop.Serilog`    
其他代码分层是支撑层，如果自己业务涉及不到，可以删除。

#### API接口层主要是基于RBAC的基于按钮级别的角色授权逻辑，以下几个Controller，不可删除    
`BaseApiController.cs`(接口基类)、`DepartmentController`(部门)、`ImgController`(图片)、`LoginController`(登录)、`ModuleController`(接口)、`PermissionController`(菜单)、`RoleController`(角色)、`TasksQzController`(任务调度)、`UserController`(用户)、`UserRoleController`(用户角色关系)     
其他代码部分是扩展业务，如果自己业务涉及不到，可以删除。



### 自定义全部日志记录图
![系统架构图](https://img.neters.club/github/log5.png)


### 自定义(中间件/服务)启动图
![系统架构图](https://img.neters.club/github/load-tool.png)


&nbsp;


## 贡献者们 

Thanks goes to these wonderful people ([✨](https://github.com/anjoy8/MultiAgentDevelop/graphs/contributors)):（排名暂时按提交顺序）  
  
    
<a href="https://github.com/anjoy8/multiagentdevelop/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=anjoy8/multiagentdevelop" />
</a>
<!-- prettier-ignore-end -->

This project follows the [all-contributors](https://github.com/anjoy8/MultiAgentDevelop/graphs/contributors) specification.
Contributions of any kind are welcome!

&nbsp;

## 官方文档 📕

还在陆续整理中，不过基本操作都在,包括如何新手入门，配置数据，连接DB等等    

[官方文档](http://apk.neters.club/.doc/)    
[公众号重要文章+视频地址](https://mvp.neters.club/)    




&nbsp;

### 系统架构图


![系统架构图](https://img.neters.club/github/20201228135550.png)

&nbsp;

&nbsp;
### 系统压测结果报告


<div align=center><img width="500" src="http://apk.neters.club/JMeterTest.png" /></div>

本项目是 .netCore 后端部分，前端部分请看我的另三个Vue工程项目
 
&nbsp;
&nbsp;
&nbsp;
&nbsp;

|个人博客Vue版本|tBug项目Nuxt版本|VueAdmin权限管理后台|
|-|-|-|
|[https://github.com/anjoy8/Blog.Vue](https://github.com/anjoy8/Blog.Vue)|[https://github.com/anjoy8/Nuxt.tBug](https://github.com/anjoy8/Nuxt.tBug)|[https://github.com/anjoy8/Blog.Admin](https://github.com/anjoy8/Blog.Admin)|
|[http://vueblog.neters.club](http://vueblog.neters.club)|[http://tibug.neters.club](http://tibug.neters.club)|[http://vueadmin.neters.club](http://vueadmin.neters.club)|



&nbsp;

### 初始化项目
 

下载项目后，编译如果没问题，直接运行即可，会自动生成种子数据，数据库是`Sqlite`，接口文档是`swagger`。    

更多操作，点击这里：http://apk.neters.club/.doc/guide/getting-started.html


&nbsp;

## Nuget Packages

| Package | NuGet Stable |  Downloads |
| ------- | -------- | ------- |
| [MultiAgentDevelop.Webapi.Template](https://www.nuget.org/packages/MultiAgentDevelop.Webapi.Template/) | [![MultiAgentDevelop.Webapi.Template](https://img.shields.io/nuget/v/MultiAgentDevelop.Webapi.Template.svg)](https://www.nuget.org/packages/MultiAgentDevelop.Webapi.Template/)  | [![MultiAgentDevelop.Webapi.Template](https://img.shields.io/nuget/dt/MultiAgentDevelop.Webapi.Template.svg)](https://www.nuget.org/packages/MultiAgentDevelop.Webapi.Template/) |


关于如何使用，点击这里：https://www.cnblogs.com/laozhang-is-phi/p/10205495.html

&nbsp;
&nbsp;

## 其他后端框架
目前一共开源四个框架项目，感兴趣的可以看看

|单层项目|简单仓储框架|仓储+服务+接口|DDD框架|
|-|-|-|-|
|CURD+Seed|CURD+Seed+DI|CURD+Seed+DI+AOP等|DDD+EFCore+DI+EventBus等|
|[NetCore-Sugar-Demo](https://github.com/anjoy8/NetCore-Sugar-Demo)|[Blog.SplRepository.Demo](https://github.com/anjoy8/Blog.SplRepository.Demo)|[MultiAgentDevelop](https://github.com/anjoy8/MultiAgentDevelop)|[ChristDDD](https://github.com/anjoy8/ChristDDD)|
| -|[Blog-EFCore-Sqlite](https://github.com/anjoy8/Blog-EFCore-Sqlite)|- | -|


&nbsp;



&nbsp;

## 售后服务与支持  

鼓励作者，简单打赏~~         
如果你喜欢，就给作者加个鸡腿吧  

 
<img src="http://apk.neters.club/laozhangisphigood.jpg" alt="赞赏码" width="300" >
[图片若加载不出来，点这里](http://apk.neters.club/laozhangisphigood.jpg)



*****************************************************
### 文章+视频+直播

博客园：https://www.cnblogs.com/laozhang-is-phi/

 Bilibili：https://space.bilibili.com/387802716  
 
 直播间：https://live.bilibili.com/21507364

```
```


&nbsp;

如果你感觉看着这整个项目比较费劲，我单抽出来了几个子Demo，方便学习，项目地址 ：[https://github.com/anjoy8/BlogArti](https://github.com/anjoy8/BlogArti)



