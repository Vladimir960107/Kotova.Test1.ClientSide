using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    internal class Decryption_stuff
    {
        static string defaultDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static string defaultFileName = "encrypted_jwt.dat";
        public static string defaultFilePath = Path.Combine(defaultDocumentsPath, defaultFileName);
        public static string DecryptedJWTToken()
        {
            return DecryptedJWTToken(defaultFilePath);
        }

        static string? DecryptedJWTToken(string filePath)
        {

            if (!File.Exists(filePath))
            {
                return null;
            }

            byte[] encryptedData = File.ReadAllBytes(filePath);

            // Decrypt the JWT using DPAPI
            byte[] decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,  // Same optional entropy as during encryption
                DataProtectionScope.CurrentUser  // Or DataProtectionScope.LocalMachine
            );

            // Convert decrypted bytes to string
            string jwtToken = Encoding.UTF8.GetString(decryptedData);

            return jwtToken;
        }
        public static void DeleteJWTToken()
        {
            try
            {
                File.Delete(Decryption_stuff.defaultFilePath);
            }
            catch (DirectoryNotFoundException)
            {
                //Everything is Okay :)
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
