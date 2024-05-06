using System;
using System.Collections.Generic;
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
        static string defaultFilePath = Path.Combine(defaultDocumentsPath, defaultFileName);
        public static string DecryptedJWTToken()
        {
            return DecryptedJWTToken(defaultFilePath);
        }

            static string DecryptedJWTToken(string filePath)
        {

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
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
    }
}
