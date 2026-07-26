using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;

namespace UrbanRenewal.Plugins.Overlay
{
    internal static class ParcelDetailHelper
    {
        public static bool TryDescribeSelection(IAppContext context, out string text)
        {
            if (context == null)
            {
                text = "运行上下文无效。";
                return false;
            }
            return ParcelSelectionHelper.TryDescribeSelection(context.MapControl, out text);
        }
    }
}
