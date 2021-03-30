using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Helpers
{
    public class Base64
    {
        public static string DecodeBase64(String Encoded)
        {
            var encodedBytes = Convert.FromBase64String(Encoded);
            return Encoding.UTF8.GetString(encodedBytes);
        }
    }
}