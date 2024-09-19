using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    internal static class ConfigurationClass
    {
        public const string BASE_URL_DEVELOPMENT = "http://172.26.4.62:5239";

        public const string BASE_INSTRUCTIONS_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT+"/Instructions";

        public const string BASE_TASK_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Tasks";

        public const string BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/notificationHub";

        public const string DEFAULT_PATH_TO_INITIAL_INSTRUCTIONS = @"C:\Initial\Folder\Path";
    }
}
