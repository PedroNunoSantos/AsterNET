using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable CheckNamespace
namespace LINQPad
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Decompliled from LINQPad.exe for compatibility
    /// Thanks Joe Albahari @ https://www.linqpad.net
    /// </summary>
    internal class PasswordManager
    {
        private static readonly string FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string LocalUserDataFolder = Path.Combine(FolderPath, "LINQPad");
        private static readonly string _passwordFolder = Path.Combine(LocalUserDataFolder, "Passwords");

        private static string PasswordFolder
        {
            get
            {
                if (!Directory.Exists(_passwordFolder))
                    Directory.CreateDirectory(_passwordFolder);
                return _passwordFolder;
            }
        }

        public static string[] GetAllPasswordNames()
        {
            return Directory.GetFiles(PasswordFolder).Select(GetName).Where(f => f != null).ToArray();
        }

        public static void SetPassword(string name, string password)
        {
            if (password == null)
                DeletePassword(name);
            else
                File.WriteAllBytes(GetFilePath(name), Encrypt(password));
        }

        public static void DeletePassword(string name)
        {
            string filePath = GetFilePath(name);
            if (!File.Exists(filePath))
                return;
            File.Delete(filePath);
        }

        public static string GetPassword(string name)
        {
            string filePath = GetFilePath(name);
            if (!File.Exists(filePath))
                return null;
            try
            {
                return Decrypt(File.ReadAllBytes(filePath));
            }
            catch
            {
                return null;
            }
        }

        private static string GetFilePath(string name)
        {
            return Path.Combine(PasswordFolder, string.Concat(Encoding.UTF8.GetBytes(name.Trim().ToLowerInvariant()).Select(b => b.ToString("X2")).ToArray()));
        }

        private static string GetName(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                byte[] bytes = new byte[fileName.Length / 2];
                for (int index = 0; index < bytes.Length; ++index)
                    bytes[index] = Convert.ToByte(fileName.Substring(index * 2, 2), 16);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }

        private static byte[] Encrypt(string value)
        {
            byte[] numArray1 = new byte[32];
            RandomNumberGenerator.Create().GetBytes(numArray1);
            byte[] numArray2 = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), numArray1, DataProtectionScope.CurrentUser);
            return numArray1.Concat(numArray2).ToArray();
        }

        private static string Decrypt(byte[] encrypted)
        {
            byte[] array = encrypted.Take(32).ToArray();
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted.Skip(32).ToArray(), array, DataProtectionScope.CurrentUser));
        }
    }
}
