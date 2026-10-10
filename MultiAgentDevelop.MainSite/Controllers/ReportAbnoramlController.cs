using Microsoft.AspNetCore.Mvc;
using MultiAgentDevelop.Model;
using MultiAgentDevelop.Model.Models;
using SqlSugar;

namespace MultiAgentDevelop.MainSite.Controllers
{
    public class ReportAbnoramlController : Controller
    {
        private readonly IConfiguration _configuration;

        public ReportAbnoramlController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string keyword = "")
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;
            keyword = keyword?.Trim() ?? string.Empty;

            using var db = CreateDb();
            RefAsync<int> totalCount = 0;
            var query = db.Queryable<ReportAbnoraml>()
                .WhereIF(!string.IsNullOrWhiteSpace(keyword),
                    x => x.AbnormalCode.Contains(keyword) || x.AbnormalName.Contains(keyword) || x.ReportId.Contains(keyword))
                .OrderBy(x => x.OrderId)
                .OrderBy(x => x.AbnormalCode);

            var data = await query.ToPageListAsync(page, pageSize, totalCount);
            var model = new PageModel<ReportAbnoraml>(page, totalCount, pageSize, data);

            ViewBag.Keyword = keyword;
            return View(model);
        }

        private SqlSugarClient CreateDb()
        {
            var mainDb = _configuration["MainDB"] ?? "Main";
            var dbOption = _configuration.GetSection("DBS")
                .GetChildren()
                .Select(DbConnectionOptions.FromConfiguration)
                .FirstOrDefault(x => x.Enabled && string.Equals(x.ConnId, mainDb, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"未找到启用的主库配置：{mainDb}");

            return new SqlSugarClient(new ConnectionConfig
            {
                ConfigId = dbOption.ConnId,
                DbType = (SqlSugar.DbType)dbOption.DBType,
                ConnectionString = dbOption.Connection,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
        }

        private sealed class DbConnectionOptions
        {
            public string ConnId { get; set; } = string.Empty;

            public int DBType { get; set; }

            public bool Enabled { get; set; }

            public string Connection { get; set; } = string.Empty;

            public static DbConnectionOptions FromConfiguration(IConfigurationSection section)
            {
                return new DbConnectionOptions
                {
                    ConnId = section["ConnId"] ?? string.Empty,
                    DBType = int.TryParse(section["DBType"], out var dbType) ? dbType : 0,
                    Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled,
                    Connection = section["Connection"] ?? string.Empty
                };
            }
        }
    }
}
