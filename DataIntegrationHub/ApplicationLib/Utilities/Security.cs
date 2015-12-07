using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Collections;

namespace ApplicationLib.Utilities
{
    public static class Security
    {
        public static String GetMD5Hash(String input)
        {
            if (String.IsNullOrEmpty(input))
                return "";
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] originalBytes = Encoding.Default.GetBytes(input);
            Byte[] encodedBytes = md5.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes);
        }

        public static String GetSHA256Hash(String input)
        {
            if (String.IsNullOrEmpty(input))
                return "";
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            Byte[] originalBytes = Encoding.Default.GetBytes(input);
            Byte[] encodedBytes = sha256.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes);
        }

        static Byte[] bytes = ASCIIEncoding.ASCII.GetBytes("Z3nN5Alt");
        /// <summary>Encrypt a String</summary>    
        public static String Encrypt(String originalString)
        {
            if (String.IsNullOrEmpty(originalString))
            {
                throw new ArgumentNullException("The String which needs to be encrypted can not be null.");
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);

            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        /// <summary>Decrypt a crypted String.</summary>    
        public static String Decrypt(String cryptedString)
        {
            if (String.IsNullOrEmpty(cryptedString))
            {
                throw new ArgumentNullException("The String which needs to be decrypted can not be null.");
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }

        public static String GeneratePWD()
        {
            return GeneratePWD(9);
        }

        public static String GeneratePWD(int len)
        {
            Int32 passwordLength = len;
            Int32 quantity = 1;
            ArrayList arrCharPool = new ArrayList();
            Random rndNum = new Random();
            arrCharPool.Clear();
            String password = "";

            for (Int32 i = 97; i < 123; i++) //Lower Case
            {
                arrCharPool.Add(Convert.ToChar(i).ToString());
            }
            for (Int32 i = 48; i < 58; i++) //Number
            {
                arrCharPool.Add(Convert.ToChar(i).ToString());
            }
            for (Int32 x = 0; x < quantity; x++)
            {
                for (Int32 i = 0; i < passwordLength; i++)
                {
                    password += arrCharPool[rndNum.Next(arrCharPool.Count)].ToString();
                }
            }
            return password;
        }
    }
}
