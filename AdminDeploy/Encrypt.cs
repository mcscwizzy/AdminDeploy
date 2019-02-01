using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace AdminDeploy
{
    class Encrypt
    {
        // program path
        static private string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\AdminDeploy";

        // unencrypt password
        static public string Unprotect()
        {
            //place holder for cipher
            string password = null;

            try
            {
                // decrypt pasword
                byte[] cipherBytes = Convert.FromBase64String(ReturnCipher());
                byte[] saltBytes = Encoding.Unicode.GetBytes(ReturnSalt().ToCharArray());
                byte[] passwordBytes = ProtectedData.Unprotect(cipherBytes, saltBytes, DataProtectionScope.LocalMachine);
                password = Encoding.Unicode.GetString(passwordBytes);
            }
            catch(Exception e)
            {
                throw new Exception(e.ToString());
            }

            // return plain text password
            return password;
        }

        // return salt for encryption
        static private string ReturnSalt()
        {
            return File.ReadAllText(path + "\\salt.txt");
        }

        // return cipher for encryption
        static private string ReturnCipher()
        {
            return File.ReadAllText(path + "\\cipher.txt");
        }
    }
}
