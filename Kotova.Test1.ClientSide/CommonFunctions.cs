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
    public static class CommonRoleNamesForCoordinatorForms
    {
        public static readonly Dictionary<string, string> RoleDisplayNames = new Dictionary<string, string>
        {
            { "user", "Сотрудник" },
            { "chief of department", "Руководство ОТДЕЛА" },
            { "coordinator", "Охрана труда" },
            { "management", "Руководство ФИЛИАЛА" }
        };

        // And an inverse dictionary if we need to convert back:
        public static readonly Dictionary<string, string> RoleDBNames = new Dictionary<string, string>
        {
            { "Сотрудник", "user" },
            { "Руководство ОТДЕЛА", "chief of department" },
            { "Охрана труда", "coordinator" },
            { "Руководство ФИЛИАЛА", "management" }
        };
    }


}
