using System.Globalization;
using System.Text;

namespace UrbanRenewal.Model
{
  public sealed class ExternalRasterCheckResult
  {
    public bool IsCompatible { get; set; }

    public bool SpatialReferenceMismatch { get; set; }

    public bool CellSizeMismatch { get; set; }

    public string ExpectedSpatialReference { get; set; }

    public string ActualSpatialReference { get; set; }

    public double ExpectedCellSize { get; set; }

    public double ActualCellSizeX { get; set; }

    public double ActualCellSizeY { get; set; }

    public string SummaryMessage { get; set; }

    public string BuildDialogMessage()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("外部交通栅格与当前分析环境不一致，无法继续。");
      sb.AppendLine("本程序不会自动重投影或重采样，请在外部 GIS 中处理后再导入。");
      sb.AppendLine();
      sb.AppendLine("【坐标系】");
      sb.AppendLine("　系统要求：" + (ExpectedSpatialReference ?? "（未设定）"));
      sb.AppendLine("　当前栅格：" + (ActualSpatialReference ?? "（无法读取）"));
      sb.AppendLine();
      sb.AppendLine("【像元大小】");
      sb.AppendLine("　系统要求：" + ExpectedCellSize.ToString("0.##", CultureInfo.InvariantCulture) + " m");
      sb.AppendLine("　当前栅格：X="
          + ActualCellSizeX.ToString("0.####", CultureInfo.InvariantCulture)
          + " m，Y="
          + ActualCellSizeY.ToString("0.####", CultureInfo.InvariantCulture) + " m");
      sb.AppendLine();
      sb.AppendLine("建议操作（ArcGIS）：");
      sb.AppendLine("　1. 投影：数据管理工具 → 投影和变换 → 投影栅格");
      sb.AppendLine("　2. 重采样：数据管理工具 → 栅格 → 栅格处理 → 重采样");
      sb.AppendLine("　　 像元大小设为 "
          + ExpectedCellSize.ToString("0.##", CultureInfo.InvariantCulture)
          + "，与全局设置一致");
      sb.AppendLine("　3. 处理完成后重新选择栅格并运行动力性分析");
      return sb.ToString();
    }
  }
}
