using System.Security.Cryptography;
using System.Text;

namespace Xieyi.ORM.Core.Helper
{
    internal static class MD5Helper
    {
        public static string GetMd5Hash(string source)
        {
            using (var md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                var sBuilder = new StringBuilder();
                
                foreach (var t in data)
                    sBuilder.Append(t.ToString("x2"));
               
                return sBuilder.ToString();
            }
        }

        public static bool VerifyMd5Hash(string md5HashString, string source)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(GetMd5Hash(source), md5HashString) == 0;
        }
    }
}
