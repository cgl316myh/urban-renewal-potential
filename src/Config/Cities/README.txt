中间缓冲/栅格与最终结果写入「输出 GDB」（动力性分析窗体可设；默认同目录 Motivation_Output.gdb）。

换城市快速上手
================

1. 打开目标城市 File GDB
2. 动力性分析 →「从GDB生成」：按通用关键词自动草拟 Config\Cities\<Id>.xml
3. 用记事本微调图层 name / keywords（至少保证 StudyArea 正确）
4. 下拉选择该城市 →「检测匹配」确认
5. 空间参考必须统一（数据完整性检查会警告不一致图层）
6. 开始分析

也可手工：复制 _Template.xml 为 CityName.xml，填写各 Layer 的 name/keywords。

role 说明见 _Template.xml 注释。权重可用 0~1 或百分数。
当前选用的城市 Id 会记在 _active_city.txt，完整性检查会按该配置校验。
