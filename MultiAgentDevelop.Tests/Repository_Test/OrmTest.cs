using System;
using Autofac;
using MultiAgentDevelop.Common.Extensions;
using MultiAgentDevelop.IRepository.Base;
using MultiAgentDevelop.Model.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SqlSugar;
using Xunit;
using Xunit.Abstractions;

namespace MultiAgentDevelop.Tests;

public class OrmTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IBaseRepository<SysUserInfo> _baseRepository;
    DI_Test dI_Test = new DI_Test();

    public OrmTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var container = dI_Test.DICollections();

        _baseRepository = container.Resolve<IBaseRepository<SysUserInfo>>();
        _baseRepository.Db.Aop.OnLogExecuting = (sql, p) =>
        {
            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine("==================FullSql=====================", "", new string[] { sql.GetType().ToString(), GetParas(p), "【SQL语句】：" + sql });
            _testOutputHelper.WriteLine("【SQL语句】：" + sql);
            _testOutputHelper.WriteLine(GetParas(p));
            _testOutputHelper.WriteLine("==============================================");
            _testOutputHelper.WriteLine("");
        };
    }

    private static string GetParas(SugarParameter[] pars)
    {
        string key = "【SQL参数】：";
        foreach (var param in pars)
        {
            key += $"{param.ParameterName}:{param.Value}\n";
        }

        return key;
    }

    [Fact]
    public void MultiTables()
    {
        var sql = _baseRepository.Db.Queryable<SysUserInfo>()
            .AS($@"{nameof(SysUserInfo)}_TenantA")
            .ToSqlString();
        //_testOutputHelper.WriteLine(sql);

        _baseRepository.Db.MappingTables.Add(nameof(SysUserInfo), $@"{nameof(SysUserInfo)}_TenantA");

        var query = _baseRepository.Db.Queryable<SysUserInfo>()
            .LeftJoin<UserRole>((a, c) => a.Id == c.UserId);

        sql = query.ToSqlString();

        _testOutputHelper.WriteLine(sql);

        sql = _baseRepository.Db.Deleteable<SysUserInfo>().ToSqlString();
        _testOutputHelper.WriteLine(sql);
    }
}