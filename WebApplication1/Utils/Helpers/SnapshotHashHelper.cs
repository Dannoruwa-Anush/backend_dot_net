using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WebApplication1.Utils.Helpers
{
    public static class SnapshotHashHelper
    {
        public static string SerializeCanonical<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }

        public static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }

        public static string BuildHash(string json)
        {
            return ComputeHash(json);
        }
    }
}