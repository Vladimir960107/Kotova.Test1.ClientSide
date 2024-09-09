using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    public static class Constants
    {
        public static string favicon_path = "Kotova.Test1.ClientSide.static.LynKS_favicon.ico";
        public static string logo_path = "Kotova.Test1.ClientSide.static.LynKS_logo.png";
    }
    public class TreeViewWithoutDoubleClick : TreeView
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0203)
            {
                m.Msg = 0x0201;
            }
            base.WndProc(ref m);
        }
    }
    public class CheckedListBoxWithoutDoubleClick: CheckedListBox
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0203)
            {
                m.Msg = 0x0201;
            }
            base.WndProc(ref m);
        }
    }


}
