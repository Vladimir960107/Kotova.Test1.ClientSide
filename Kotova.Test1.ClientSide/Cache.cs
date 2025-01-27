using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Cache
{
    public static Dictionary<string, string> KeyValueTable { get; private set; } = new Dictionary<string, string>();

    public static void Load(Dictionary<string, string> data)
    {
        KeyValueTable = data;
    }

    public static string GetValue(string key)
    {
        return KeyValueTable.TryGetValue(key, out var value) ? value : null;
    }
}
