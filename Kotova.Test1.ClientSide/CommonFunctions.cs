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
    public static class CommonRoleNamesForCoordinatorForms // TODO: СДЛЕАТЬ ЭТО В БАЗЕ ДАННЫХ И В КЛАСС CACHE, И СКАЧИВАТЬ СЮДА В ПРОГРАММУ ИЗ БАЗЫ ДАННЫХ!
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

    public static class InstructionTypeMappings
    {
        // Maps numeric IDs to instruction names
        public static readonly Dictionary<int, string> IdToName = new Dictionary<int, string>
        {
            { 0, "Вводный" },
            { 1, "Внеплановый" },
            { 2, "Первичный" },
            { 3, "Первичный (для водителей)" },
            { 4, "Целевой" }
        };

            // Maps instruction names to numeric IDs
            public static readonly Dictionary<string, int> NameToId = new Dictionary<string, int>
        {
            { "Вводный", 0 },
            { "Внеплановый", 1 },
            { "Первичный", 2 },
            { "Первичный (для водителей)", 3 },
            { "Целевой", 4 }
        };

        // Helper method to get name from ID
        public static string GetInstructionName(int id)
        {
            return IdToName.TryGetValue(id, out string name) ? name : "Unknown";
        }

        // Helper method to get ID from name
        public static int GetInstructionId(string name)
        {
            return NameToId.TryGetValue(name, out int id) ? id : -1;
        }
    }

    public static class ComboBoxExtensions
    {
        public static void PreventMouseWheelScroll(this System.Windows.Controls.ComboBox comboBox)
        {
            comboBox.PreviewMouseWheel += (s, e) =>
            {
                e.Handled = true;
            };
        }
    }


}
