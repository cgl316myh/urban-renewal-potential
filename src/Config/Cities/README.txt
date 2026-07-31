全局工作区设置（Config/app_settings.xml）
========================================
- OutputGdbPath：所有分析结果（中间+结果）写入此 File GDB
- ActiveCityProfileId：当前城市图层角色配置
- InputGdbPath：最近打开的输入 GDB
- CellSize：潜力分析统一像元大小（米，默认 30；动力性/可行度/叠置共用）

在程序中：「数据管理 → 全局设置」修改并保存即可，动力性/可行度等模块自动使用。

换城市快速上手
================
1. 打开目标城市 File GDB
2. 数据管理 → 全局设置 →「从GDB生成」草拟城市配置
3. 指定输出 GDB（可点「默认」）→ 保存
4. 动力性分析 → 运行动力性分析（结果 mot_score）
5. 可行度分析 → 运行可行度分析（结果 fea_score；需宗地/DEM/人口）
6. 叠置评价 → 运行综合潜力叠置（结果 pot_score / pot_level）
7. 宗地关联 → 运行宗地关联（结果 parcel_pot，写入潜力字段）
8. 验证校核 → 对标已更新宗地（需 UpdatedParcel 图层）
9. 成果输出 → 导出 TIFF/SHP/CSV；地图 PDF/TIFF 专题图
10. 系统配置 → 调整叠置/准则层权重与皮肤

也可手工：复制 Cities\_Template.xml 为 CityName.xml，填写图层 name/keywords。
可行度角色：Parcel、DEM、Slope（可选）、Population。
验证角色：UpdatedParcel（已更新宗地）。
