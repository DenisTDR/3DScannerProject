using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UtilisAndExtensionsLibrary
{
    public static class Meths
    {
        public static string CalculateMd5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(i.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetPropertyJsonName(Type objectType, Type propertyType)
        {
            return
                objectType.GetProperties()
                    .FirstOrDefault(x => x.PropertyType == propertyType)?
                    .CustomAttributes.FirstOrDefault(
                        x => x.AttributeType == typeof (JsonPropertyAttribute))?
                    .NamedArguments?.FirstOrDefault(x => x.MemberName == "PropertyName")
                    .TypedValue.Value.ToString() ?? propertyType.Name;
        }
    }
}
