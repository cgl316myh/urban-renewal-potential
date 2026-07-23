全局工作区设置（Config/app_settings.xml）
========================================
- OutputGdbPath：所有分析结果（中间+结果）写入此 File GDB
- ActiveCityProfileId：当前城市图层角色配置
- InputGdbPath：最近打开的输入 GDB

在程序中：「数据管理 → 全局设置」修改并保存即可，动力性等模块自动使用。

换城市快速上手
================
1. 打开目标城市 File GDB
2. 数据管理 → 全局设置 →「从GDB生成」草拟城市配置
3. 指定输出 GDB（可点「默认」）→ 保存
4. 动力性分析 → 开始分析（结果写入全局输出 GDB）

也可手工：复制 Cities\_Template.xml 为 CityName.xml，填写图层 name/keywords。
