using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Kotova.Test1.ClientSide
{
    internal static class Notifications
    {
        public static void ShowWindowsNotification(string title, string message)
        {
            string tempImagePath = Path.Combine(Path.GetTempPath(), "tempImage.png");

            try
            {
                // Ensure any previous temp image file is deleted
                if (File.Exists(tempImagePath))
                {
                    File.Delete(tempImagePath);
                }

                // Access the logo stream from resources
                using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Constants.logo_path))
                {
                    if (iconStream != null)
                    {
                        // Convert the icon to an image and save it as a PNG file
                        using (var image = Image.FromStream(iconStream))
                        {
                            image.Save(tempImagePath); // Save as PNG
                        }
                    }
                    else
                    {
                        MessageBox.Show("Logo path not found in assembly resources.");
                        return;
                    }
                }

                // Build and display the notification
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddInlineImage(new Uri(tempImagePath)) // Add image path
                    .AddButton(new ToastButton()
                        .SetContent("Open App")
                        .AddArgument("action", "openApp")) // Pass arguments to the app
                    .Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying notification: {ex.Message}");
            }
        }
    }
}
