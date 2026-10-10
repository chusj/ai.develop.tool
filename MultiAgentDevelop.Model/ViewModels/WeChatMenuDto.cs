using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentDevelop.Model.ViewModels
{
    /// <summary>
    /// 获取微信菜单DTO
    /// </summary>
    public class WeChatMenuDto
    {
        /// <summary>
        /// 按钮列表(最多三个)
        /// </summary>
        public WeChatMenuButtonDto[] button { get; set; }

    }
}
