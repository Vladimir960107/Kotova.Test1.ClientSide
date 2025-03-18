using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json; // Add this namespace

namespace Kotova.Test1.ClientSide
{
    public class DepartmentCache
    {
        private string _cachePath;
        private Dictionary<int, string> _departments;

        // Singleton pattern for global access
        private static DepartmentCache _instance;
        public static DepartmentCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DepartmentCache();
                }
                return _instance;
            }
        }

        public DepartmentCache(string cachePath = null)
        {
            _cachePath = cachePath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YourAppName",
                "departments_cache.json");

            _departments = LoadCache();
        }

        public string GetDepartmentName(int departmentId)
        {
            return _departments.TryGetValue(departmentId, out var name) ? name : $"Department {departmentId}";
        }

        public void UpdateCache(Dictionary<int, string> departments)
        {
            _departments = departments;
            SaveCache();
        }

        private Dictionary<int, string> LoadCache()
        {
            try
            {
                if (File.Exists(_cachePath))
                {
                    var json = File.ReadAllText(_cachePath);
                    return JsonSerializer.Deserialize<Dictionary<int, string>>(json) ?? new Dictionary<int, string>();
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }

            return new Dictionary<int, string>();
        }

        private void SaveCache()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_cachePath));
                var json = JsonSerializer.Serialize(_departments);
                File.WriteAllText(_cachePath, json);
            }
            catch (Exception)
            {
                // Log exception if needed
            }
        }
    }
}