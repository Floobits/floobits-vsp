using System;

namespace Floobits.Common
{
    class DigestUtils
    {
        public static string md5Hex(string data)
        {
            return String.Concat(Array.ConvertAll(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.UTF8.GetBytes(data)), x => x.ToString("X2"))).ToLower();
        }
        public static string md5Hex(byte[] data)
        {
            return String.Concat(Array.ConvertAll(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data), x => x.ToString("X2"))).ToLower();
        }
    }

}