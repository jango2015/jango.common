using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jango.Common.Utilities
{
    public class Function
    {
        public static string Base64Encode(string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(str));
        }
    }
}
