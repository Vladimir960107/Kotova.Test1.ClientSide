using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public static class RoleMappings
    {
        private static readonly Dictionary<string, string> _roleDbToDisplay = new Dictionary<string, string> // TODO: СДЛЕАТЬ ЭТО В БАЗЕ ДАННЫХ И В КЛАСС CACHE, И СКАЧИВАТЬ СЮДА В ПРОГРАММУ ИЗ БАЗЫ ДАННЫХ!
        {
            { "user", "Сотрудник" },
            { "chief of department", "Руководство ОТДЕЛА" },
            { "coordinator", "Охрана труда" },
            { "management", "Руководство ФИЛИАЛА" }
        };

        private static readonly Dictionary<string, string> _roleDisplayToDb;

        // ReadOnly dictionaries for public access
        public static ReadOnlyDictionary<string, string> RoleDbToDisplay { get; }
        public static ReadOnlyDictionary<string, string> RoleDisplayToDb { get; }

        static RoleMappings()
        {
            // Initialize the reverse mapping
            _roleDisplayToDb = new Dictionary<string, string>();
            foreach (var kvp in _roleDbToDisplay)
            {
                _roleDisplayToDb[kvp.Value] = kvp.Key;
            }

            // Create readonly versions for public access
            RoleDbToDisplay = new ReadOnlyDictionary<string, string>(_roleDbToDisplay);
            RoleDisplayToDb = new ReadOnlyDictionary<string, string>(_roleDisplayToDb);
        }

        /// <summary>
        /// Converts a database role name to its display name.
        /// </summary>
        /// <param name="dbRoleName">The database role name</param>
        /// <returns>The display name if found, null if not found</returns>
        public static string? GetRoleDisplayName(string? dbRoleName)
        {
            if (string.IsNullOrEmpty(dbRoleName))
                return null;

            return _roleDbToDisplay.TryGetValue(dbRoleName, out string? displayName) ? displayName : null;
        }

        /// <summary>
        /// Converts a display role name to its database name.
        /// </summary>
        /// <param name="displayRoleName">The display role name</param>
        /// <returns>The database role name if found, null if not found</returns>
        public static string? GetRoleDbName(string? displayRoleName)
        {
            if (string.IsNullOrEmpty(displayRoleName))
                return null;

            return _roleDisplayToDb.TryGetValue(displayRoleName, out string? dbName) ? dbName : null;
        }

        /// <summary>
        /// Gets all available display role names.
        /// </summary>
        /// <returns>A collection of display role names</returns>
        public static IEnumerable<string> GetAllDisplayNames()
        {
            return _roleDbToDisplay.Values;
        }

        /// <summary>
        /// Gets all available database role names.
        /// </summary>
        /// <returns>A collection of database role names</returns>
        public static IEnumerable<string> GetAllDbNames()
        {
            return _roleDbToDisplay.Keys;
        }
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

    public static class DepartmentMappings
    {
        private static readonly Dictionary<string, int> _departmentNameToId = new Dictionary<string, int>
        {
            { "Общестроительный отдел", 1 },
            { "Технический отдел", 2 },
            { "Руководство", 5 }
        };

        private static readonly Dictionary<int, string> _departmentIdToName;

        // ReadOnly dictionaries for public access
        public static ReadOnlyDictionary<string, int> DepartmentNameToId { get; }
        public static ReadOnlyDictionary<int, string> DepartmentIdToName { get; }

        static DepartmentMappings()
        {
            // Initialize the reverse mapping
            _departmentIdToName = new Dictionary<int, string>();
            foreach (var kvp in _departmentNameToId)
            {
                _departmentIdToName[kvp.Value] = kvp.Key;
            }

            // Create readonly versions for public access
            DepartmentNameToId = new ReadOnlyDictionary<string, int>(_departmentNameToId);
            DepartmentIdToName = new ReadOnlyDictionary<int, string>(_departmentIdToName);
        }

        /// <summary>
        /// Converts a department name to its corresponding ID.
        /// </summary>
        /// <param name="departmentName">The name of the department</param>
        /// <returns>The department ID if found, null if not found</returns>
        public static int? GetDepartmentId(string? departmentName)
        {
            if (string.IsNullOrEmpty(departmentName))
                return null;

            return _departmentNameToId.TryGetValue(departmentName, out int id) ? id : null;
        }

        /// <summary>
        /// Converts a department ID to its corresponding name.
        /// </summary>
        /// <param name="departmentId">The ID of the department</param>
        /// <returns>The department name if found, null if not found</returns>
        public static string? GetDepartmentName(int? departmentId)
        {
            if (!departmentId.HasValue)
                return null;

            return _departmentIdToName.TryGetValue(departmentId.Value, out string name) ? name : null;
        }
    }


}
