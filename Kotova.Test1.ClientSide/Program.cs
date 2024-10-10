using System.IO.Pipes;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    internal static class Program
    {
        private static Mutex mutex = null;

        [STAThread]
        static void Main()
        {
            const string mutexName = "LynksApp";
            bool isNewInstance = false;

            mutex = new Mutex(true, mutexName, out isNewInstance);

            if (!isNewInstance)
            {
                NotifyExistingInstance();  // Notify the existing instance and exit
                return;
            }

            // Start the named pipe server to listen for further instances
            StartNamedPipeServer();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();

            Login_Russian loginForm = new Login_Russian();
            Application.Run(loginForm);

            GC.KeepAlive(mutex);
        }

        private static void StartNamedPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("LynksAppPipe", PipeDirection.In))
                    {
                        server.WaitForConnection();
                        // Signal received from another instance; bring the main window to front
                        Login_Russian.Instance?.ShowForm();
                        server.Disconnect();
                    }
                }
            });
        }

        private static void NotifyExistingInstance()
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", "LynksAppPipe", PipeDirection.Out))
                {
                    client.Connect(1000);  // Timeout of 1 second
                    // Successfully notified the existing instance
                }
            }
            catch (TimeoutException)
            {
                // Could not connect to the existing instance in time
                MessageBox.Show("Failed to notify the existing instance.");
            }
        }
    }
}
