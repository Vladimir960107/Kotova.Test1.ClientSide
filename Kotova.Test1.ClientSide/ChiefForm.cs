using System.Net.Http.Headers;

namespace Kotova.Test1.ClientSide
{
    public partial class ChiefForm : Form
    {
        static string? selectedFolderPath = null;
        private Form? _loginForm;
        string? _userName;
        public ChiefForm()
        {
            InitializeComponent();
        }
        public ChiefForm(Form loginForm, string userName)
        {
            _loginForm = loginForm;
            _userName = userName;
            InitializeComponent();
        }
        private void buttonChooseHyperLinkToInstruction_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // Optionally set the initial directory
                // folderBrowserDialog.SelectedPath = @"C:\Initial\Folder\Path";

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // Store the selected folder path in a variable
                    selectedFolderPath = folderBrowserDialog.SelectedPath;

                    PathToFolderOfInstruction.Text = selectedFolderPath;


                    // Use the selectedFolderPath variable as needed in your code
                    MessageBox.Show($"Selected Folder: {selectedFolderPath}");

                }
            }
        }

        private async void testButton_Click(object sender, EventArgs e)
        {
            string url = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/greeting";
            await Test.connectionToUrlGet(url);
        }
    }
}
