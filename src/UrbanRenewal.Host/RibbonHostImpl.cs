using System;
using System.Drawing;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// DevExpress 13.1 Ribbon 挂接实现。
    /// </summary>
    internal sealed class RibbonHostImpl : IRibbonHost
    {
        private readonly RibbonControl _ribbon;

        public RibbonHostImpl(RibbonControl ribbon)
        {
            _ribbon = ribbon;
        }

        public object AddPage(string pageName)
        {
            RibbonPage page = new RibbonPage(pageName);
            _ribbon.Pages.Add(page);
            return page;
        }

        public object AddGroup(object page, string groupName)
        {
            RibbonPage ribbonPage = page as RibbonPage;
            if (ribbonPage == null)
            {
                throw new ArgumentException("page 必须为 RibbonPage", "page");
            }

            RibbonPageGroup group = new RibbonPageGroup(groupName);
            ribbonPage.Groups.Add(group);
            return group;
        }

        public object AddButton(object group, string caption, EventHandler clickHandler)
        {
            RibbonPageGroup ribbonGroup = group as RibbonPageGroup;
            if (ribbonGroup == null)
            {
                throw new ArgumentException("group 必须为 RibbonPageGroup", "group");
            }

            BarButtonItem item = new BarButtonItem();
            item.Caption = caption;
            item.Name = "btn_" + Math.Abs(caption.GetHashCode()).ToString();
            ApplyLargeImage(item, caption);

            if (clickHandler != null)
            {
                item.ItemClick += delegate(object sender, ItemClickEventArgs e)
                {
                    clickHandler(sender, EventArgs.Empty);
                };
            }

            _ribbon.Items.Add(item);
            ribbonGroup.ItemLinks.Add(item);
            return item;
        }

        /// <summary>
        /// 为设计器中的地图按钮等补上大图标。
        /// </summary>
        public static void ApplyLargeImage(BarButtonItem item, string caption)
        {
            if (item == null)
            {
                return;
            }
            Image large = RibbonIconFactory.GetLargeByCaption(caption ?? item.Caption);
            item.LargeGlyph = large;
            item.Glyph = large;
            item.RibbonStyle = RibbonItemStyles.Large;
        }
    }
}
