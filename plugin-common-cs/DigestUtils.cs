using System;

namespace Floobits.Common
{
    class DigestUtils
    {
        public static string md5Hex(string data)
        {
            return Convert.ToBase64String(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.UTF8.GetBytes(data)));
        }
        public static string md5Hex(byte[] data)
        {
            return Convert.ToBase64String(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data));
        }
    }

}