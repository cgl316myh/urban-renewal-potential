using System;

namespace UrbanRenewal.Contracts
{
    /// <summary>
    /// 封装 DevExpress Ribbon，供插件动态挂接页签/分组/按钮。
    /// </summary>
    public interface IRibbonHost
    {
        object AddPage(string pageName);

        object AddGroup(object page, string groupName);

        object AddButton(object group, string caption, EventHandler clickHandler);
    }
}
