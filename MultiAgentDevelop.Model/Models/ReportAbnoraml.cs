using SqlSugar;

namespace MultiAgentDevelop.Model.Models
{
    /// <summary>
    /// 报告异常信息
    /// </summary>
    [SugarTable("REPORT_ABNORAML")]
    public class ReportAbnoraml
    {
        /// <summary>
        /// 机构ID
        /// </summary>
        [SugarColumn(ColumnName = "ORGAN_ID", Length = 40, IsNullable = false)]
        public string OrganId { get; set; }

        /// <summary>
        /// 报告ID
        /// </summary>
        [SugarColumn(ColumnName = "REPORT_ID", Length = 40, IsNullable = false)]
        public string ReportId { get; set; }

        /// <summary>
        /// 异常编码
        /// </summary>
        [SugarColumn(ColumnName = "ABNORMAL_CODE", Length = 6, IsNullable = false)]
        public string AbnormalCode { get; set; }

        /// <summary>
        /// 异常名称
        /// </summary>
        [SugarColumn(ColumnName = "ABNORMAL_NAME", Length = 100, IsNullable = false)]
        public string AbnormalName { get; set; }

        /// <summary>
        /// 异常类型
        /// </summary>
        [SugarColumn(ColumnName = "ABNORMAL_TYPE", IsNullable = true)]
        public decimal? AbnormalType { get; set; }

        /// <summary>
        /// 紧急性，原预警等级
        /// </summary>
        [SugarColumn(ColumnName = "DANGER_LEVEL", IsNullable = true)]
        public decimal? DangerLevel { get; set; }

        /// <summary>
        /// 重要性，原就医期限
        /// </summary>
        [SugarColumn(ColumnName = "MEDICAL_LIMIT", IsNullable = true)]
        public decimal? MedicalLimit { get; set; }

        /// <summary>
        /// 医学解释
        /// </summary>
        [SugarColumn(ColumnName = "MEDICAL_EXPLAIN", Length = 4000, IsNullable = true)]
        public string MedicalExplain { get; set; }

        /// <summary>
        /// 常见原因
        /// </summary>
        [SugarColumn(ColumnName = "COMMON_CAUSE", Length = 4000, IsNullable = true)]
        public string CommonCause { get; set; }

        /// <summary>
        /// 建议
        /// </summary>
        [SugarColumn(ColumnName = "ADVICE", Length = 4000, IsNullable = true)]
        public string Advice { get; set; }

        /// <summary>
        /// 排序ID
        /// </summary>
        [SugarColumn(ColumnName = "ORDER_ID", IsNullable = true)]
        public decimal? OrderId { get; set; }
    }
}
